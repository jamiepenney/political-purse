using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace PoliticalPurse.Web.Infrastructure
{
    [HtmlTargetElement("script", Attributes = "custom-prepend-cdn", TagStructure = TagStructure.WithoutEndTag)]
    public class CdnScriptBundleHelper : ScriptTagHelper
    {
        [HtmlAttributeName("custom-prepend-cdn")]
        public string PrependCdn { get; set; }

        public CdnScriptBundleHelper(ILogger<CdnScriptBundleHelper> logger,
            IHostingEnvironment hostingEnvironment,
            IMemoryCache cache,
            HtmlEncoder htmlEncoder,
            JavaScriptEncoder javaScriptEncoder,
            IUrlHelperFactory urlHelper)
            : base(hostingEnvironment, cache, htmlEncoder, javaScriptEncoder, urlHelper) {}

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            if(base.HostingEnvironment.IsProduction()){
                output.Attributes.SetAttribute("src", PrependCdn + output.Attributes["src"].Value);
            }
        }
    }
}