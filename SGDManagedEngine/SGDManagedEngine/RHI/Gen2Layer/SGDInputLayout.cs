using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGDManagedEngine.SGD.Gen2Layer
{
    // mirroring with H1VertexDeclaration
    public partial class H1InputLayout
    {
        public H1InputElementDescription[] InputElements
        {
            get { return m_InputElements.ToArray(); }
        }

        public H1InputLayout(H1VertexStream[] streams)
        {
            foreach (H1VertexStream stream in streams)
            {
                H1InputElementDescription inputElement = new H1InputElementDescription();
                inputElement.SemanticName = stream.SemanticName;
                inputElement.SemanticIndex = stream.SemanticIndex;
                inputElement.Format = H1RHIDefinitionHelper.ConvertToPixelFormat(stream.VertexElementType);
                inputElement.InputSlot = Convert.ToInt32(stream.Offset);

                m_InputElements.Add(inputElement);
            }

            // create platform-dependent input-layout
            CreateInputLayout();
        }

        private List<H1InputElementDescription> m_InputElements = new List<H1InputElementDescription>();
    }
}
