// Copyright 2007-2008 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Serialization.Custom.TypeSerializers
{
    using System.Xml;

    public class CharSerializer :
		SerializerBase<char>
	{
		protected override void WriteValue(XmlWriter writer, object value)
		{
			var c = (char) value;
			if(c == default(char))
				return;

			writer.WriteValue(XmlConvert.ToString(c));
		}
	}
}