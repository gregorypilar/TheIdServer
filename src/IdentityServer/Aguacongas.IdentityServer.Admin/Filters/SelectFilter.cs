﻿using Aguacongas.IdentityServer.Store;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Community.OData.Linq;
using Community.OData.Linq.Json;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aguacongas.IdentityServer.Admin.Filters
{
    class SelectFilter : ResultFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var controlerType = context.Controller.GetType();
            var result = context.Result as ObjectResult;
            var value = result?.Value;
            var query = context.HttpContext.Request.Query;
            if (value != null &&
                query.ContainsKey("select") &&
                controlerType.FullName
                .StartsWith("Aguacongas.IdentityServer.Admin.GenericApiController",
                    StringComparison.Ordinal))
            {
                var valueType = value.GetType();
                var entityType = controlerType.GetGenericArguments()[0];
                var pageResponseType = typeof(PageResponse<>).MakeGenericType(entityType);
                if (pageResponseType == valueType)
                {
                    var items = pageResponseType.GetProperty("Items").GetValue(value);
                    var selectResultType = typeof(SelectResult<>).MakeGenericType(entityType);
                    var selectResult = selectResultType.GetConstructors()[0]
                        .Invoke(Array.Empty<object>()) as SelectResult;
                    var pageResponse = new SelectedPageResponse
                    {
                        Items = selectResult.Select(items, query["select"], query["expand"]),
                        Count = (int)pageResponseType.GetProperty("Count").GetValue(value)
                    };
                    result.Value = pageResponse;
                }
            }

            await next();
        }

        class SelectResult<T>: SelectResult
        {
            private readonly static CamelCasePropertyNamesContractResolver _resolver = new CamelCasePropertyNamesContractResolver();
            public override JToken Select(object items, string select, string expand)
            {
                return Select(items as IEnumerable<T>, select, expand);
            }

            private JToken Select(IEnumerable<T> items, string select, string expand)
            {
                return items.AsQueryable().OData().SelectExpand(select, expand).ToJson(options => options.ContractResolver = _resolver);
            }
        }

        abstract class SelectResult
        {
            public abstract JToken Select(object items, string select, string expand);
        }

        class SelectedPageResponse
        {
            public int Count { get; set; }

            public JToken Items { get; set; }
        }
    }
}