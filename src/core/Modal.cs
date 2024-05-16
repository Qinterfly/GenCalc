using System.Collections.Generic;

namespace GenCalc.Core.Numerical
{
    public class ModalDataSet
    {
        public bool isCorrect()
        {
            if (!isEmpty())
            {
                const int kBaseIndex = 0;
                string forceRun = Forces[kBaseIndex].OriginalRun;
                string responseRun = Responses[kBaseIndex].OriginalRun;
                string referenceRun = ReferenceResponse.OriginalRun;
                return forceRun == responseRun && responseRun == referenceRun;
            }
            else
            {
                return false;
            }
        }

        private bool isEmpty()
        {
            return Forces == null || Responses == null || ReferenceResponse == null;
        }

        public List<int> getForceLinks()
        {
            int numForces = Forces.Count;
            int numResponses = Responses.Count;
            if (numForces == 0 || numResponses == 0)
                return null;
            List<int> links = new List<int>(numForces);
            for (int iForce = 0; iForce != numForces; ++iForce)
            {
                Response force = Forces[iForce];
                links.Add(-1);
                for (int iResponse = 0; iResponse != numResponses; ++iResponse)
                {
                    Response response = Responses[iResponse];
                    if (force.Node == response.Node && force.Direction == response.Direction)
                    {
                        links[iForce] = iResponse;
                        break;
                    }
                }
                if (links[iForce] < 0)
                    return null;
            }
            return links;
        }

        public List<Response> Forces = null;
        public List<Response> Responses = null;
        public Response ReferenceResponse = null;
    }

    public class ModalParameters
    {
        public ModalParameters()
        {
            Levels = new List<double>();
            Mass = new List<double>();
            Stiffness = new List<double>();
            Damping = new List<double>();
            Frequency = new List<double>();
        }

        public readonly List<double> Levels;
        public readonly List<double> Mass;
        public readonly List<double> Stiffness;
        public readonly List<double> Damping;
        public readonly List<double> Frequency;
    }
}
