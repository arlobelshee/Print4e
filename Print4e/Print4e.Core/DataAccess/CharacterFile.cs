using System.Xml.Linq;
using JetBrains.Annotations;

namespace Print4e.Core.DataAccess
{
	public class CharacterFile
	{
		[NotNull] private readonly XElement _data;

		private CharacterFile([NotNull] XElement data)
		{
			_data = data;
		}

		public static CharacterFile FromXml([NotNull] string fileContents)
		{
			return new CharacterFile(XElement.Parse(fileContents));
		}
	}
}