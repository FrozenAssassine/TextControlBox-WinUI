using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextControlBoxNS.Renderer;
using Windows.UI;

namespace TextControlBoxNS.Text
{
    internal class PropertiesManager
    {
        private TextRenderer textRenderer;
        public PropertiesManager(TextRenderer textRenderer)
        {
            this.textRenderer = textRenderer;
        }

        public bool _ShowLineNumbers = true;
        public bool _ShowLineHighlighter = true;


        public float _SpaceBetweenLineNumberAndText = 30;
    }
}
