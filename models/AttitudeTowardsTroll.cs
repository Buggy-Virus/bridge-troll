using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BridgeTroll
{
    public struct Bounty {
        public float amount;
    }

    public class AttitudeTowardsTroll
    {
        public float terror;
        public float terror_recent;
        public float trust;
        public float trust_recent;
        public float dependability;
        public float dependability_recent;
        public float service;
        public float service_recent;
        public float infamy;
        public float infamy_recent;

        public List<Bounty> bounties;

        private void DecayFunction() {

        }

        private void DecayFunctionRecent() {

        }

        // This will take the events of the day.
        public void UpdateAttitude() {

        }
    }
}