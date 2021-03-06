﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Ivony.Http.Pipeline
{


  /// <summary>
  /// implement a combinator to combine HTTP pipeline and asp.net core server
  /// </summary>
  public class AspNetCoreCombinator : IHttpPipelineAccessPoint<RequestDelegate>
  {
    internal static readonly string HttpContextAccessKey = "__HttpContext";


    protected virtual async Task ApplyResponse( HttpContext context, HttpResponseMessage response )
    {

      context.Response.StatusCode = (int) response.StatusCode;
      context.Response.Headers.Clear();

      var ignores = response.Headers.Connection;

      foreach ( var item in response.Headers )
      {
        if ( ignoreHeaders.Contains( item.Key ) )
          continue;

        if ( ignores.Contains( item.Key ) )
          continue;

        context.Response.Headers.Add( item.Key, new StringValues( item.Value.ToArray() ) );
      }

      foreach ( var item in response.Content.Headers )
      {
        if ( ignoreHeaders.Contains( item.Key ) )
          continue;

        if ( ignores.Contains( item.Key ) )
          continue;

        context.Response.Headers.Add( item.Key, new StringValues( item.Value.ToArray() ) );
      }

      await response.Content.CopyToAsync( context.Response.Body );

    }


    protected virtual Task<HttpRequestMessage> CreateRequest( HttpContext context )
    {
      var request = CreateRequestCore( context );

      request.Method = new HttpMethod( context.Request.Method );
      request.RequestUri = CreateUri( context.Request );


      var ignores = request.Headers.Connection;

      foreach ( var item in context.Request.Headers )
      {
        if ( ignoreHeaders.Contains( item.Key ) )
          continue;

        if ( ignores.Contains( item.Key ) )
          continue;

        request.Headers.Add( item.Key, item.Value.AsEnumerable() );
      }

      return Task.FromResult( request );
    }


    protected readonly static HashSet<string> ignoreHeaders = new HashSet<string>( StringComparer.OrdinalIgnoreCase ) { "Accept-Encoding", "Connection", "Content-Encoding", "Content-Length", "Keep-Alive", "Transfer-Encoding", "TE", "Accept-Transfer-Encoding", "Trailer", "Upgrade", "Proxy-Authorization", "Proxy-Authenticate" };


    protected virtual Uri CreateUri( HttpRequest request )
    {
      var builder = new UriBuilder();
      builder.Scheme = request.Scheme;
      if ( request.Host.HasValue )
      {
        builder.Host = request.Host.Host;
        if ( request.Host.Port.HasValue )
          builder.Port = request.Host.Port.Value;
      }

      builder.Path = request.Path;
      builder.Query = request.QueryString.Value;

      return builder.Uri;
    }


    private static HttpRequestMessage CreateRequestCore( HttpContext context )
    {
      var request = new HttpRequestMessage();
      request.Properties[HttpContextAccessKey] = context;
      return request;
    }

    RequestDelegate IHttpPipelineAccessPoint<RequestDelegate>.Combine( IHttpPipelineHandler pipeline )
    {
      if ( pipeline == null )
        throw new ArgumentNullException( nameof( pipeline ) );

      return async context =>
      {
        var request = await CreateRequest( context );

        var response = await pipeline.ProcessRequest( request );

        await ApplyResponse( context, response );
      };
    }
  }
}