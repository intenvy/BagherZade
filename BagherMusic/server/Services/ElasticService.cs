// Standard
using System;
using System.Collections.Generic;
using System.Linq;

// Internal
using BagherMusic.Core.Elastic;
using BagherMusic.Core.QuerySystem;
using BagherMusic.Models;
using BagherMusic.Utils;

// Microsoft
using Microsoft.Extensions.Configuration;

// Elastic
using Nest;

namespace BagherMusic.Services
{
	public abstract class ElasticService<T, G> : IElasticService<T, G> where G : IEntity<T>
	{
		protected IElasticClient client;

		protected int resultCountPerPage;
		protected int acceptableFuzziness;

		protected string indexName;
		protected string[] searchFields;
		protected string[] foreignFields;

		public ElasticService(IConfiguration config, IElasticClientService clientService)
		{
			client = clientService.GetInstance();
			resultCountPerPage = Int32.Parse(config["ElasticService:Options:ResultCountPerPage"]);
			acceptableFuzziness = Int32.Parse(config["ElasticService:Options:AcceptableFuzziness"]);
		}

		private void CreateInitialClient(string elasticUri)
		{
			var uri = new Uri(elasticUri);
			var connectionSettings = new ConnectionSettings(uri);
			client = new ElasticClient(connectionSettings);
		}

		public G GetEntity(T id)
		{
			var queryContainer = BuildMatchQueryContainer(id.ToString(), "_id");
			var response = client.Search<G>(s => s
				.Index(indexName)
				.Query(q => queryContainer)
				.Size(1)
			);
			Validator.Validate(response);

			return response.Documents.ToList<G>() [0];
		}

		public HashSet<G> GetEntities(Object foreignKey)
		{
			var queryContainer = BuildMustQueryContainer(new Query(foreignKey.ToString()), foreignFields);
			var response = Search(queryContainer, 0, 10_000);

			return response.Documents.ToList<G>().ToHashSet();
		}

		public HashSet<G> GetSearchResults(Query query, int pageIndex)
		{
			var queryContainer = BuildSearchQueryContainer(query, searchFields);
			var response = Search(queryContainer, pageIndex);

			return response.Documents.ToList<G>().ToHashSet();
		}

		private QueryContainer BuildMatchQueryContainer(string query, string field)
		{
			return new MatchQuery
			{
				Field = field,
					Query = query
			};
		}

		private QueryContainer BuildMustQueryContainer(Query query, string[] fields)
		{
			return new BoolQuery
			{
				Must = query.Ands.Tokens.Select(
					token => (QueryContainer) new MultiMatchQuery
					{
						Fields = fields,
							Query = token.Id
					}
				)
			};
		}

		private QueryContainer BuildSearchQueryContainer(Query query, string[] fields)
		{
			return new BoolQuery
			{
				Must = query.Ands.Tokens.Select(
						token => (QueryContainer) new MultiMatchQuery
						{
							Fields = fields,
								Query = token.Id,
								Fuzziness = Fuzziness.EditDistance(acceptableFuzziness)
						}
					),
					Should = query.Ors.Tokens.Select(
						token => (QueryContainer) new MultiMatchQuery
						{
							Fields = fields,
								Query = token.Id,
								Fuzziness = Fuzziness.EditDistance(acceptableFuzziness)
						}
					),
					MustNot = query.Excs.Tokens.Select(
						token => (QueryContainer) new MultiMatchQuery
						{
							Fields = fields,
								Query = token.Id,
								Fuzziness = Fuzziness.EditDistance(acceptableFuzziness)
						}
					)
			};
		}

		private ISearchResponse<G> Search(QueryContainer queryContainer, int from, int size)
		{
			var response = client.Search<G>(s => s
				.Index(indexName)
				.Query(q => queryContainer)
				.From(from)
				.Size(size)
			);
			Validator.Validate(response);

			return response;
		}

		private ISearchResponse<G> Search(QueryContainer queryContainer, int pageIndex)
		{
			return Search(queryContainer, pageIndex * resultCountPerPage, resultCountPerPage);
		}

		public int Import(string resourcesPath)
		{
			var bulk = CreateBulk(FileHandler.GetEntitiesFromFolder<T, G>(resourcesPath));
			var response = client.Bulk(bulk);
			Validator.Validate(response);
			return response.Items.Count;
		}

		private BulkDescriptor CreateBulk(IEnumerable<G> entities)
		{
			var bulkDescriptor = new BulkDescriptor();
			bulkDescriptor.IndexMany<G>(entities, (descriptor, s) => descriptor.Index(indexName).Id(s.Id.ToString()));
			return bulkDescriptor;
		}
	}
}
