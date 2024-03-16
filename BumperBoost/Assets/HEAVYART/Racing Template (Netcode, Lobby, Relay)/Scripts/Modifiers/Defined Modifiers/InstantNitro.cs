using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HEAVYART.Racing.Netcode
{
    public class InstantNitro : InstantModifier
    {
        public float nitroAmount;

        public InstantNitro()
        {
            type = GetType().ToString();
        }

        protected override void SerializeModifier()
        {
            object[] outputData = new object[] { nitroAmount };

            serializedData = Newtonsoft.Json.JsonConvert.SerializeObject(outputData);
        }

        protected override ModifierBase DeserializeModifier(string inputData)
        {
            object[] data = Newtonsoft.Json.JsonConvert.DeserializeObject<object[]>(inputData);

            nitroAmount = Convert.ToSingle(data[0]);

            return this;
        }
    }
}
