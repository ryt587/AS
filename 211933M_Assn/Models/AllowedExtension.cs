using System.ComponentModel.DataAnnotations;

namespace _211933M_Assn.Models
{
	public class AllowedExtension : ValidationAttribute
	{
		private readonly string[] _extensions;

		public AllowedExtension(string[] extensions)
		{
			_extensions = extensions;
		}


		public override bool IsValid(object value)
		{
			if (value is null)
				return true;

			var file = value as IFormFile;
			var extension = Path.GetExtension(file.FileName);

			if (!_extensions.Contains(extension.ToLower()))
				return false;

			return true;
		}
	}
}
