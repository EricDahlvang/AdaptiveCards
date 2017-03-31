﻿using System.Collections.Generic;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace AdaptiveCards
{
    /// <summary>
    ///     The ImageSet allows for the inclusion of a collection images like a photogallery.
    /// </summary>
    public class ImageSet : CardElement
    {
        public ImageSet()
        {
            Type = "ImageSet";
        }

        /// <summary>
        ///     Collection of images to display together
        /// </summary>
        [JsonRequired]
#if NET45
        [XmlElement(ElementName="Image", Type=typeof(Image))]
#endif
        public List<Image> Images { get; set; } = new List<Image>();

        /// <summary>
        ///     Specifies the horizontal size of each image in the set
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
#if NET45
        [XmlAttribute]
#endif
        public ImageSize ImageSize { get; set; }

        public bool ShouldSerializeImageSize()
        {
            return ImageSize != ImageSize.Auto;
        }
    }
}