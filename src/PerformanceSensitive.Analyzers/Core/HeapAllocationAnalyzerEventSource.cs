namespace ClrHeapAllocationAnalyzer
{
    using System.Diagnostics.Tracing;

    internal sealed class HeapAllocationAnalyzerEventSource : EventSource
    {
        public static HeapAllocationAnalyzerEventSource Logger = new HeapAllocationAnalyzerEventSource();

        public void StringConcatenationAllocation(string filePath)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(1, filePath);
            }
        }

        public void BoxingAllocationInStringConcatenation(string filePath)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(2, filePath);
            }
        }

        public void NewInitializerExpression(string filePath)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(3, filePath);
            }
        }

        public void NewImplicitArrayCreationExpression(string filePath)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(4, filePath);
            }
        }

        public void NewAnonymousObjectCreationExpression(string filePath)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(5, filePath);
            }
        }

        public void NewObjectCreationExpression(string filePath)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(6, filePath);
            }
        }

        public void LetClauseExpression(string filePath)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(7, filePath);
            }
        }

        public void ParamsAllocation(string filePath)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(8, filePath);
            }
        }

        public void NonOverridenVirtualMethodCallOnValueType(string filePath)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(9, filePath);
            }
        }

        public void BoxingAllocation(string filePath)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(10, filePath);
            }
        }

        public void MethodGroupAllocation(string filePath)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(11, filePath);
            }
        }

        public void EnumeratorAllocation(string filePath)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(12, filePath);
            }
        }

        public void ClosureCapture(string filePath)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(13, filePath);
            }
        }

        public void NewArrayExpression(string filePath)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(14, filePath);
            }
        }

        public void ReadonlyMethodGroupAllocation(string filePath) {
            if (this.IsEnabled()) {
                this.WriteEvent(15, filePath);
            }
        }
    }
}