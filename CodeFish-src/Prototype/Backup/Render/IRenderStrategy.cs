using System;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;

namespace Prototype
{	
	public interface IRenderStrategy
	{
		Layout GenerateLayout();
	}
}
