using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace poc_azure_function.Request
{
    public class TestRequest
    {
        //[Required(ErrorMessage = "名前は必須です。")]
        [Display(Name = "名前")]
        [Required(ErrorMessageResourceName = "Require", ErrorMessageResourceType = typeof(Resource.Messages))]
        [MaxLength(10)]
        [MinLength(3)]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [Display(Name = "年齢")]
        [Required(ErrorMessageResourceName = "Require", ErrorMessageResourceType = typeof(Resource.Messages))]
        [Range(20, 30)]
        [JsonProperty(PropertyName = "age")]
        public string Age { get; set; }

        [Display(Name = "郵便番号")]
        [Required(ErrorMessageResourceName = "Require", ErrorMessageResourceType = typeof(Resource.Messages))]
        [RegularExpression(@"^[0-9]{3}-[0-9]{4}$")]
        [JsonProperty(PropertyName = "zip_code")]
        public string ZipCode { get; set; }

        [Display(Name = "誕生日")]
        [Required(ErrorMessageResourceName = "Require", ErrorMessageResourceType = typeof(Resource.Messages))]
        [JsonProperty(PropertyName = "birth")]
        public DateTime BirthDay { get; set; }
    }
}
