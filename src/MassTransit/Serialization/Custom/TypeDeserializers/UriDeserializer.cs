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
namespace Burrows.Serialization.Custom.TypeDeserializers
{
    using System;
    using System.Runtime.Serialization;

    public class UriDeserializer :
		IObjectDeserializer<Uri>
	{
		public object Deserialize(IDeserializerContext context)
		{
			string uriString = context.ReadElementAsString();
			if (uriString == null)
				return null;

			try
			{
				var uri = new Uri(uriString);

				return uri;
			}
			catch (UriFormatException ex)
			{
				throw new SerializationException("The Uri is in an invalid format: " + uriString, ex);
			}
		}
	}
}