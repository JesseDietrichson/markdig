// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Markdig.Extensions.CustomContainers
{
    /// <summary>
    /// A HTML renderer for a <see cref="CustomContainer"/>.
    /// </summary>
    /// <seealso cref="HtmlObjectRenderer{CustomContainer}" />
    public class HtmlCustomContainerRenderer : HtmlObjectRenderer<CustomContainer>
    {
        protected override void Write(HtmlRenderer renderer, CustomContainer obj)
        {
            renderer.EnsureLine();

            if (obj.Info == "image")
            {
                //just take alt-text and source from obj.Arguments
                //html should be src and alt
                //img ends with just >
                if (renderer.EnableHtmlForBlock)
                {
                    (string alt, string src, string border) = extractParamatersFromImage(obj.Arguments);
                    renderer
                        .Write("<p><img ")
                        .Write(string.IsNullOrEmpty(alt) ? "" : "alt=" + alt + " ")
                        .Write("src=" + src + " ")
                        .Write("border=" + border)
                        .Write("/></p>");
                }
                renderer.WriteChildren(obj);
            }
            else
            {
                if (renderer.EnableHtmlForBlock)
                {
                    renderer.Write("<div").WriteAttributes(obj).Write(">");
                }
                // We don't escape a CustomContainer
                renderer.WriteChildren(obj);
                if (renderer.EnableHtmlForBlock)
                {
                    renderer.WriteLine("</div>");
                }
            }
        }

        private (string alt, string src, string border) extractParamatersFromImage(string text)
        {
            string altText = "";
            string source = "";
            string border = "";

            //extract alt text
            if (text.Contains("alt-text="))
            {
                string value = extractValueFromText(text, "alt-text=", "\"", "\"", true);
                altText = value;
            }
            //extract source
            if (text.Contains("source="))
            {
                string value = extractValueFromText(text, "source=", "\"", "\"", true);
                source = value;
            }
            //extract border
            if (text.Contains("border="))
            {
                string value = extractValueFromText(text, "border=", "\"", "\"", true);
                border = value;
            }
            else //if no border paramater is there we need to add one. This is the default behavior when using :::image
                border = "\"true\"";

            return (altText, source, border);
        }

        private string extractValueFromText(string text, string referencePoint, string firstBound, string secondBound, bool includeBounds = false)
        {
            try
            {
                int referencePointIndex = text.IndexOf(referencePoint);
                string secondHalfOfFile = text.Substring(referencePointIndex);
                int firstBoundIndex = secondHalfOfFile.IndexOf(firstBound) + firstBound.Length;
                string thirdHalfOfFile = secondHalfOfFile.Substring(firstBoundIndex);
                if (secondBound == "")
                    return thirdHalfOfFile;
                int secoundBoundIndex = thirdHalfOfFile.IndexOf(secondBound);
                if (secoundBoundIndex == -1)
                {
                    secoundBoundIndex = thirdHalfOfFile.Length - 1;
                }
                string finalSubString = thirdHalfOfFile.Substring(0, secoundBoundIndex);
                if (includeBounds)
                {
                    finalSubString = firstBound + finalSubString + secondBound;
                }
                finalSubString = finalSubString.Replace("\n", "");
                return finalSubString;
            }
            catch
            {
                return null;
            }
        }
    }
}