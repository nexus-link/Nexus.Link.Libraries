#if NETCOREAPP

using System;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
// ReSharper disable MemberCanBePrivate.Global

namespace Nexus.Link.Libraries.Web.AspNet.Annotations
{
    public static class SwaggerExtensions
    {
        /// <summary>
        /// In order to group entities by capability, use <see cref="CapabilityGroupingAttribute"/> on the controllers,
        /// and activate with this method in the Startup process.
        ///
        /// This will setup TagActionsBy(...) to use <see cref="CapabilityGroupingAttribute"/>,
        /// and then OrderActionsBy(...) by relative part.
        ///
        /// Fallback is the controller name (as per default).
        /// </summary>
        public static SwaggerGenOptions EnableCapabilityGrouping(this SwaggerGenOptions options)
        {
            options.TagActionsBy(api =>
            {
                if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                {
                    // Try to find CapabilityGrouping on the controller itself first.
                    // If that fails, find it on the controllers base type (which we often use in Contract libraries)
                    var tag = (CapabilityGroupingAttribute)Attribute.GetCustomAttribute(controllerActionDescriptor.ControllerTypeInfo, typeof(CapabilityGroupingAttribute)) ??
                              (controllerActionDescriptor.ControllerTypeInfo.BaseType != null ? (CapabilityGroupingAttribute)Attribute.GetCustomAttribute(controllerActionDescriptor.ControllerTypeInfo.BaseType, typeof(CapabilityGroupingAttribute)) : null);
                    if (tag != null)
                    {
                        return new[] { tag.Description };
                    }

                    // Default to controller name (typically an entity name)
                    return new[] { controllerActionDescriptor.ControllerName };
                }

                throw new InvalidOperationException("Unable to determine tag for endpoint.");
            });

            // Order the actions by relative path, so that we take into account the capability part of the path
            options.OrderActionsBy(x => x.RelativePath);

            return options;
        }


        [Obsolete("Use UseCapabilityGrouping instead", true)]
        public static SwaggerUIOptions EnableCapabilityGrouping(this SwaggerUIOptions options)
        {
            return UseCapabilityGrouping(options, "h4");
        }

        /// <summary>
        /// Adds javascript to HeadContent that will group sections according to the use of <see cref="CapabilityGroupingAttribute"/>.
        ///
        /// Uses DOM manipulation.
        /// </summary>
        public static SwaggerUIOptions UseCapabilityGrouping(this SwaggerUIOptions options, string headerTag = "h3")
        {
            var script = WrapSwaggerHeadScript(
                "        const groupSections = () => {\n" +
                "            let hash = [];\n" +
               $"            document.querySelectorAll('{headerTag}.opblock-tag').forEach(headerTag => {{" +
                "                const tag = headerTag.getAttribute('data-tag');\n" +
                "                const span = headerTag.parentNode.parentNode;\n" +
               $"                if (tag.indexOf('{CapabilityGroupingAttribute.DescriptionDivder}') !== -1) {{\n" +
               $"                    const heading = tag.split('{CapabilityGroupingAttribute.DescriptionDivder}')[0].trim();\n" +
                "                    if (!hash[heading]) hash[heading] = [];\n" +
                "                    hash[heading].push(span);\n" +
                "                } else hash[tag] = span;\n" +
                "            });\n" +
                "            let idx = 0;\n" +
                "            for (const key of Object.keys(hash)) {\n" +
                "                idx++;\n" +
                "                const el = hash[key];\n" +
                "                if (el.length) {\n" +
                "                    for (let i = 0; i < el.length; i++) {\n" +
                "                        const section = el[i];\n" +
                "                        const textEl = section.querySelector('a span');\n" +
               $"                        textEl.innerText = textEl.innerText.split('{CapabilityGroupingAttribute.DescriptionDivder}')[1].trim();\n" +

                "                        const parent = section.parentNode;\n" +
                "                        const containerId = `section-container-${idx}`;\n" +
                "                        let container = document.querySelector(`#${containerId}`);\n" +
                "                        if (!container) {\n" +
                "                            container = document.createElement('div');\n" +
                "                            container.id = containerId;\n" +
                "                            container.style = 'border: 1px solid #ccc; border-radius: 0.5rem; padding: 0 1rem; margin-bottom: 1rem;';\n" +
                "                            container.innerHTML = `<h3>${key}</h3>`;\n" +
                "                            parent.insertAdjacentElement('afterbegin', container); \n" +
                "                        }\n" +
                "                        container.insertAdjacentElement('beforeend',section);\n" +
                "                    }\n" +
                "                }\n" +
                "            }\n" +
                "        }\n" +
                "        groupSections();\n");

            if (options.HeadContent == null) options.HeadContent = "";
            options.HeadContent += script;

            return options;
        }

        [Obsolete("Use UseSectionToggling", true)]
        public static SwaggerUIOptions EnableSectionToggling(this SwaggerUIOptions options, bool sectionsInitiallyClosed = true)
        {
            return UseSectionToggling(options, sectionsInitiallyClosed, "h4");
        }

        /// <summary>
        /// Adds javascript to HeadContent that will add a "Toggle sections" button that will expand/collapse all sections at once.
        /// </summary>
        public static SwaggerUIOptions UseSectionToggling(this SwaggerUIOptions options, bool sectionsInitiallyClosed = true, string headerTag = "h3")
        {
            var script = WrapSwaggerHeadScript(
                "        const toggleButton = document.createElement('button');\n" +
                "        toggleButton.classList.add('btn');\n" +
                "        toggleButton.innerHTML = 'Toggle sections';\n" +
               $"        toggleButton.addEventListener('click', event => {{ document.querySelectorAll('{headerTag}').forEach(x => x.click()); }});\n" +
                "        authorizeButton.insertAdjacentElement('afterend', toggleButton);\n" +
                (sectionsInitiallyClosed ? "        toggleButton.click();" : ""));

            if (options.HeadContent == null) options.HeadContent = "";
            options.HeadContent += script;

            return options;
        }

        private static string WrapSwaggerHeadScript(string javascriptcode)
        {
            return "<script>\n" +
                   "window.addEventListener('DOMContentLoaded', event => {\n" +
                   "    const handle = setInterval(() => {\n" +
                   "        const authorizeButton = document.querySelector('.btn.authorize');\n" +
                   "        if (!authorizeButton) return;\n" + // Button appears after some 500 ms, and then the UI is ready for us
                   "        clearInterval(handle);\n" +

                   javascriptcode +

                   "    }, 100);\n" +
                   "});\n" +
                   "</script>\n";
        }
    }

}

#endif
