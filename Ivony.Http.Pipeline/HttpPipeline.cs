﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Http.Pipeline
{

  /// <summary>
  /// 辅助实现 IHttpPipeline 接口
  /// </summary>
  public abstract class HttpPipeline : IHttpPipeline
  {


    /// <summary>
    /// downstream pipeline
    /// </summary>
    protected HttpPipelineHandler Downstream { get; private set; }

    /// <summary>
    /// join downstream pipeline
    /// </summary>
    /// <param name="downstream">downstream pipeline</param>
    /// <returns></returns>
    public HttpPipelineHandler Join( HttpPipelineHandler downstream )
    {
      Downstream = downstream;
      return request => ProcessRequest( request );
    }


    /// <summary>
    /// 通过方法创建一个 IHttpPipeline 对象
    /// </summary>
    /// <param name="pipeline">一个处理管线方法</param>
    /// <returns></returns>
    public static IHttpPipeline Create( Func<HttpPipelineHandler, HttpPipelineHandler> pipeline )
    {
      return new PipelineWrapper( pipeline );
    }


    /// <summary>
    /// 派生类重写此方法处理请求
    /// </summary>
    /// <param name="request">请求信息</param>
    /// <returns>响应信息</returns>
    protected virtual Task<HttpResponseMessage> ProcessRequest( HttpRequestMessage request )
    {
      return Downstream( request );
    }



    /// <summary>
    /// 获取一个空白管道，该管道不在管线上增加任何操作
    /// </summary>
    public static IHttpPipeline Blank { get; } = new BlankPipeline();


    private class BlankPipeline : IHttpPipeline
    {
      public HttpPipelineHandler Join( HttpPipelineHandler downstream )
      {
        return downstream;
      }

      public override string ToString()
      {
        return "::";
      }
    }

    private class PipelineWrapper : IHttpPipeline
    {
      private Func<HttpPipelineHandler, HttpPipelineHandler> _pipeline;

      public PipelineWrapper( Func<HttpPipelineHandler, HttpPipelineHandler> pipeline )
      {
        _pipeline = pipeline;
      }

      public HttpPipelineHandler Join( HttpPipelineHandler downstream )
      {
        return _pipeline( downstream );
      }
    }
  }
}
