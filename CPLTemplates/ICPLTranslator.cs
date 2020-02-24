using System;
using System.Threading.Tasks;

namespace BizSpeed.CPLTemplates
{
	public interface ICPLTranslator
	{
		string Translate(string textToTranslate);
	}
}

