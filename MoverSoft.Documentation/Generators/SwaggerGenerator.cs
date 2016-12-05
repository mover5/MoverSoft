using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using MoverSoft.Common.Extensions;
using MoverSoft.Documentation.Attributes;
using MoverSoft.Documentation.Swagger;
using Newtonsoft.Json;

namespace MoverSoft.Documentation.Generators
{
    public class SwaggerGenerator
    {
        private static List<RequestQueryAttribute> GlobalQueryStrings { get; set; }

        private static List<RequestHeaderAttribute> GlobalHeaders { get; set; }

        public static SwaggerInfo GlobalApiInfo { get; set; }

        private IApiExplorer ApiExplorer { get; set; }

        private SwaggerInfo ApiInfo { get; set; }

        private Dictionary<Type, Schema> DefinitionRefmap { get; set; }

        public SwaggerGenerator(IApiExplorer apiExplorer, SwaggerInfo info)
        {
            this.ApiExplorer = apiExplorer;
            this.ApiInfo = info;
            this.DefinitionRefmap = new Dictionary<Type, Schema>();
        }

        public SwaggerGenerator(HttpConfiguration configuration, SwaggerInfo info)
            : this(configuration.Services.GetApiExplorer(), info)
        {
        }

        static SwaggerGenerator()
        {
            SwaggerGenerator.GlobalHeaders = new List<RequestHeaderAttribute>();
            SwaggerGenerator.GlobalQueryStrings = new List<RequestQueryAttribute>();
        }

        public SwaggerDefinition GenerateSwagger(
            string hostName,
            string[] urlSchemes,
            string basePath = "/")
        {
            if (this.ApiExplorer == null)
            {
                throw new ArgumentNullException("apiExplorer", "The api explorer must be defined");
            }

            if (this.ApiInfo == null || string.IsNullOrEmpty(this.ApiInfo.Title) || string.IsNullOrEmpty(this.ApiInfo.Version))
            {
                throw new ArgumentNullException("apiInfo", "The swagger api info must be set when constructing the generator. Title and Version are required.");
            }

            if (!urlSchemes.CoalesceEnumerable().Any())
            {
                throw new ArgumentException("UrlSchemes must contain at least 1 schemes. Example schemes are 'http', 'https', etc.");
            }

            var swagger = new SwaggerDefinition
            {
                Host = hostName,
                Schemes = urlSchemes,
                BasePath = basePath,
                Info = this.ApiInfo,
                Consumes = new string[] { "application/json" },
                Produces = new string[] { "application/json" },
            };

            swagger.Paths = this.GenerateSwaggerPaths();
            swagger.Definitions = this.GenerateSwaggerDefinitions();

            return swagger;
        }

        private Dictionary<string, Path> GenerateSwaggerPaths()
        {
            var paths = new Dictionary<string, Path>();
            foreach (var api in this.ApiExplorer.ApiDescriptions)
            {
                var pathKey = string.Format("/{0}", api.RelativePath);
                Path existingPath = null;
                if (paths.ContainsKey(pathKey))
                {
                    existingPath = paths[pathKey];
                }
                else
                {
                    paths.Add(pathKey, null);
                }

                paths[pathKey] = this.GetOrUpdatePath(api, existingPath);
            }

            return paths;
        }

        private Dictionary<string, Schema> GenerateSwaggerDefinitions()
        {
            return this.DefinitionRefmap
                .ToDictionary(keySelector: kvp => kvp.Key.Name.ToLowerInvariant(), elementSelector: kvp => kvp.Value);
        }

        private Path GetOrUpdatePath(ApiDescription api, Path path = null)
        {
            path = path ?? new Path();

            var operation = new Operation
            {
                Description = api.ActionDescriptor.ActionName,
                Summary = api.ActionDescriptor.ActionName,
                OperationId = api.ActionDescriptor.ActionName,
                Parameters = this.GetParameters(api),
                Responses = this.GetResponses(api)
            };

            switch (api.HttpMethod.ToString().ToLowerInvariant())
            {
                case "get":
                    path.Get = operation;
                    break;
                case "post":
                    path.Post = operation;
                    break;
                case "put":
                    path.Put = operation;
                    break;
                case "delete":
                    path.Delete = operation;
                    break;
            }

            return path;
        }

        private Parameter[] GetParameters(ApiDescription api)
        {
            var parameterList = new List<Parameter>();

            // Path Parameters
            parameterList.AddRange(api.ParameterDescriptions
                .Where(param => param.Source == ApiParameterSource.FromUri)
                .Select(param => new Parameter
                {
                    In = "path",
                    Name = param.Name,
                    Required = true,
                    Type = "string"
                }));

            // Query String Parameters
            if (SwaggerGenerator.GlobalQueryStrings != null)
            {
                parameterList.AddRange(SwaggerGenerator.GlobalQueryStrings
                    .Select(query => new Parameter
                    {
                        In = "query",
                        Name = query.Name,
                        Description = query.Description,
                        Required = query.Required,
                        Type = "string"
                    }));
            }

            foreach (var query in api.ActionDescriptor.GetCustomAttributes<RequestQueryAttribute>(true))
            {
                parameterList.Add(new Parameter
                {
                    In = "query",
                    Name = query.Name,
                    Description = query.Description,
                    Required = query.Required,
                    Type = "string"
                });
            }

            foreach (var query in api.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<RequestQueryAttribute>(true))
            {
                parameterList.Add(new Parameter
                {
                    In = "query",
                    Name = query.Name,
                    Description = query.Description,
                    Required = query.Required,
                    Type = "string"
                });
            }

            // Headers
            if (SwaggerGenerator.GlobalHeaders != null)
            {
                parameterList.AddRange(SwaggerGenerator.GlobalHeaders
                    .Select(header => new Parameter
                    {
                        In = "header",
                        Name = header.Name,
                        Description = header.Description,
                        Required = header.Required,
                        Type = "string"
                    }));
            }

            foreach (var header in api.ActionDescriptor.GetCustomAttributes<RequestHeaderAttribute>(true))
            {
                parameterList.Add(new Parameter
                {
                    In = "header",
                    Name = header.Name,
                    Description = header.Description,
                    Required = header.Required,
                    Type = "string"
                });
            }

            foreach (var header in api.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<RequestHeaderAttribute>(true))
            {
                parameterList.Add(new Parameter
                {
                    In = "header",
                    Name = header.Name,
                    Description = header.Description,
                    Required = header.Required,
                    Type = "string"
                });
            }

            // Body
            var bodyParam = api.ParameterDescriptions.FirstOrDefault(param => param.Source == ApiParameterSource.FromBody);
            var bodyAttribute = api.ActionDescriptor.GetCustomAttributes<RequestBodyAttribute>(true).FirstOrDefault();
            if (bodyParam != null)
            {
                parameterList.Add(new Parameter
                {
                    Name = bodyParam.Name,
                    Description = bodyParam.Documentation,
                    In = "body",
                    Required = true,
                    Schema = this.GetRefSchemaFromType(bodyParam.ParameterDescriptor.ParameterType)
                });
            }
            else if (bodyAttribute != null)
            {
                parameterList.Add(new Parameter
                {
                    Name = !string.IsNullOrEmpty(bodyAttribute.Name) ? bodyAttribute.Name : bodyAttribute.RequestBodyType.Name,
                    Description = !string.IsNullOrEmpty(bodyAttribute.Description) ? bodyAttribute.Description : bodyAttribute.RequestBodyType.FullName,
                    In = "body",
                    Required = true,
                    Schema = this.GetRefSchemaFromType(bodyAttribute.RequestBodyType)
                });
            }

            return parameterList.ToArray();
        }

        private Dictionary<string, Response> GetResponses(ApiDescription api)
        {
            var responses = new Dictionary<string, Response>();

            foreach (var attribute in api.ActionDescriptor.GetCustomAttributes<ResponseAttribute>(true))
            {
                var key = ((int)attribute.ExpectedStatusCode).ToString();
                if (responses.ContainsKey(key))
                {
                    throw new ArgumentException(string.Format("A response already exists for {0} and status code {1}", api.RelativePath, key));
                }

                responses.Add(key, new Response
                {
                    Description = string.IsNullOrEmpty(attribute.Description) ? attribute.ResponseType.Name : attribute.Description,
                    Schema = this.GetRefSchemaFromType(attribute.ResponseType)
                });
            }

            if (!responses.Any())
            {
                responses.Add("default", new Response
                {
                    Description = "The default response."
                });
            }

            return responses;
        }

        private Schema GetRefSchemaFromType(Type type)
        {
            if (this.DefinitionRefmap.ContainsKey(type))
            {
                return new Schema
                {
                    Ref = string.Format("#/definitions/{0}", type.Name.ToLowerInvariant())
                };
            }

            var schema = this.GetSchemaFromType(type);
            if (schema.Type == "object")
            {
                return new Schema
                {
                    Ref = string.Format("#/definitions/{0}", type.Name.ToLowerInvariant())
                };
            }

            return schema;
        }

        private Schema GetSchemaFromType(Type type)
        {
            if (type == null)
            {
                return null;
            }

            var schemaType = this.GetSchemaType(type);
            var schemaFormat = this.GetSchemaFormat(type);

            var schema = new Schema
            {
                Type = schemaType,
                Format = schemaFormat,
                Items = schemaType == "array" ? this.GetSchemaFromType(this.GetArrayType(type)) : null,
                Enum = this.GetSchemaEnumValues(type),
                Properties = schemaType == "object" ? this.GetSchemaProperties(type) : null
            };

            // If schema is an object, save it and return a ref instead.
            if (schemaType == "object")
            {
                if (!this.DefinitionRefmap.ContainsKey(type))
                {
                    this.DefinitionRefmap.Add(type, schema);
                }

                return new Schema
                {
                    Ref = string.Format("#/definitions/{0}", type.Name.ToLowerInvariant())
                };
            }

            return schema;
        }

        private string[] GetSchemaEnumValues(Type type)
        {
            return type.IsEnum ? type.GetEnumNames() : null;
        }

        private Type GetArrayType(Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }

            if (type.IsGenericType && type.GetInterface("IEnumerable") != null)
            {
                return type.GetGenericArguments().FirstOrDefault();
            }

            return type;
        }

        private Dictionary<string, Schema> GetSchemaProperties(Type type)
        {
            var result = new Dictionary<string, Schema>();

            foreach (var property in type.GetProperties())
            {
                var jsonProperty = property.GetCustomAttribute<JsonPropertyAttribute>();
                var jsonIgnore = property.GetCustomAttribute<JsonIgnoreAttribute>();
                if (jsonIgnore == null)
                {
                    var name = jsonProperty != null && !string.IsNullOrEmpty(jsonProperty.PropertyName) ? jsonProperty.PropertyName : property.Name;
                    result.Add(name, this.GetSchemaFromType(property.PropertyType));
                }
            }

            return result;
        }

        private string GetSchemaType(Type type)
        {
            // TODO array
            if (type.IsArray || (type.IsGenericType && type.GetInterface("IEnumerable") != null))
            {
                return "array";
            }

            if (type == typeof(int) || type == typeof(int?) || type == typeof(long) || type == typeof(long?))
            {
                return "integer";
            }

            if (type == typeof(float) || type == typeof(float?) || type == typeof(double) || type == typeof(double?))
            {
                return "number";
            }

            if (type == typeof(bool) || type == typeof(bool?))
            {
                return "boolean";
            }

            if (type == typeof(string) || type == typeof(DateTime) || type == typeof(DateTime?) || type.IsEnum)
            {
                return "string";
            }

            return !type.IsPrimitive ? "object" : "string";
        }

        private string GetSchemaFormat(Type type)
        {
            if (type == typeof(int) || type == typeof(int?))
            {
                return "int32";
            }

            if (type == typeof(long) || type == typeof(long?))
            {
                return "int64";
            }

            if (type == typeof(float) || type == typeof(float?))
            {
                return "float";
            }

            if (type == typeof(double) || type == typeof(double?))
            {
                return "double";
            }

            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return "dateTime";
            }

            return null;
        }
    }
}
