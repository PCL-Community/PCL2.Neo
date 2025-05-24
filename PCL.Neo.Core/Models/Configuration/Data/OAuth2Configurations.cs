using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCL.Neo.Core.Models.Configuration.Data;

[ConfigurationInfo("OAuth2Configuration.json")]
public record OAuth2Configurations
{
    public string ClientId     { get; init; }
    public string ClientSecret { get; init; }
    public int    RedirectPort { get; init; }
}