using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PCL2.Neo.Service.Exceptions;

public record HttpError(
    HttpStatusCode? StatusCode,
    string Message,
    string? Content = null,
    Exception? Exception = null);