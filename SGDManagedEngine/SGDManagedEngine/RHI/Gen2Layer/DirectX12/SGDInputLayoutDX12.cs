using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D12;

namespace SGDManagedEngine.SGD.Gen2Layer
{
#if SGD_DX12
    public partial class H1InputLayout
    {
        public InputLayoutDescription Description
        {
            get { return m_InputLayoutDesc; }
        }

        void CreateInputLayout()
        {
            // @TODO - need to modify input element description from H1VertexStream

            // platform-specific implementation
            List<InputElement> inputElements = new List<InputElement>();
            foreach (var stream in m_InputElements)
            {
                InputElement element = new InputElement();
                element.SemanticName = stream.SemanticName;
                element.SemanticIndex = stream.SemanticIndex;
                element.Format = H1RHIDefinitionHelper.ConvertToFormat(stream.Format);

                // watch out - I use multiple vertex buffer instead of big vertex buffer indicating by alignbyteoffset
                element.Slot = stream.InputSlot;
                inputElements.Add(element);
            }

            // create input layout based on the resultant list of input elements
            m_InputLayoutDesc = new InputLayoutDescription(inputElements.ToArray());
        }

        private InputLayoutDescription m_InputLayoutDesc;
    }
#endif
}
