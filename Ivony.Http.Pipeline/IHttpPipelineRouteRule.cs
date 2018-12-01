﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Ivony.Http.Pipeline
{
  public interface IHttpPipelineRouteRule
  {

    IDictionary<string, string> Route( HttpRequestMessage request );

  }
}
