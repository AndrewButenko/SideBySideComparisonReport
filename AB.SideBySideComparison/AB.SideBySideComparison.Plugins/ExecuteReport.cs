using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace AB.SideBySideComparison.Plugins
{
    public class ExecuteReport : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var inDataString = (string)context.InputParameters["InData"];

            InData inData;

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(inDataString)))
            {
                DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings()
                {
                    UseSimpleDictionaryFormat = true
                };

                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(InData), settings);
                inData = (InData)ser.ReadObject(stream);
            }

            var service = serviceProvider.GetOrganizationService(context.UserId);

            var entityMetadata = ((RetrieveEntityResponse)service.Execute(new RetrieveEntityRequest()
            {
                EntityFilters = EntityFilters.Attributes,
                LogicalName = inData.entityType
            })).EntityMetadata;

            var record1 = service.Retrieve(inData.entityType, inData.Id1, new ColumnSet(true));
            var record2 = service.Retrieve(inData.entityType, inData.Id2, new ColumnSet(true));

            var result = entityMetadata.Attributes.Where(a =>
                (a.AttributeType == AttributeTypeCode.Boolean || //
                a.AttributeType == AttributeTypeCode.Customer || //
                a.AttributeType == AttributeTypeCode.DateTime || //
                a.AttributeType == AttributeTypeCode.Decimal || //
                a.AttributeType == AttributeTypeCode.Double || //
                a.AttributeType == AttributeTypeCode.Integer || //
                a.AttributeType == AttributeTypeCode.Lookup ||
                a.AttributeType == AttributeTypeCode.Memo || //
                a.AttributeType == AttributeTypeCode.Money ||
                a.AttributeType == AttributeTypeCode.Owner || //
                a.AttributeType == AttributeTypeCode.Picklist || //
                a.AttributeType == AttributeTypeCode.State || //
                a.AttributeType == AttributeTypeCode.Status || //
                a.AttributeType == AttributeTypeCode.String) &&
                string.IsNullOrEmpty(a.AttributeOf)).Select(a => GetOutData(a, record1, record2))
                .OrderBy(a => a.FieldLabel).ToArray();

            string outDataString;

            using (var stream = new MemoryStream())
            {
                DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings()
                {
                    UseSimpleDictionaryFormat = true
                };

                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(OutData[]), settings);

                ser.WriteObject(stream, result);
                outDataString = Encoding.UTF8.GetString(stream.ToArray());
            }


            context.OutputParameters["OutData"] = outDataString;
        }

        private OutData GetOutData(AttributeMetadata a, Entity record1, Entity record2)
        {
            var outData = new OutData()
            {
                FieldLabel = a.DisplayName.UserLocalizedLabel?.Label ?? a.LogicalName,
                IsPrimary = a.IsPrimaryName ?? false,
                IsCustom =  a.IsCustomAttribute ?? false
            };

            switch (a.AttributeType)
            {
                case AttributeTypeCode.Boolean:
                    outData.AttributeType = "Two Options";
                    break;
                case AttributeTypeCode.Customer:
                    outData.AttributeType = "Customer";
                    break;
                case AttributeTypeCode.DateTime:
                    outData.AttributeType = "Date and Time";
                    break;
                case AttributeTypeCode.Decimal:
                    outData.AttributeType = "Decimal Number";
                    break;
                case AttributeTypeCode.Double:
                    outData.AttributeType = "Floating Point Number";
                    break;
                case AttributeTypeCode.Integer:
                    outData.AttributeType = "Whole Number";
                    break;
                case AttributeTypeCode.Lookup:
                    outData.AttributeType = "Lookup";
                    break;
                case AttributeTypeCode.Memo:
                    outData.AttributeType = "Multiple Lines of Text";
                    break;
                case AttributeTypeCode.Money:
                    outData.AttributeType = "Currency";
                    break;
                case AttributeTypeCode.Owner:
                    outData.AttributeType = "Owner";
                    break;
                case AttributeTypeCode.Picklist:
                    outData.AttributeType = "Choice";
                    break;
                case AttributeTypeCode.State:
                    outData.AttributeType = "State Code";
                    break;
                case AttributeTypeCode.Status:
                    outData.AttributeType = "Status Code";
                    break;
                case AttributeTypeCode.String:
                    outData.AttributeType = "Single Line of Text";
                    break;
                default:
                    outData.AttributeType = $"{a.AttributeTypeName.Value} is not supported";
                    break;
            }

            var attributeName = a.LogicalName;

            if (!record1.Contains(attributeName) && !record2.Contains(attributeName))
            {
                outData.IsEqual = true;
                return outData;
            }

            object record1Value = null;
            string record1Label = null;

            if (record1.Contains(attributeName))
            {
                if (record1[attributeName] is string)
                {
                    record1Label = (string)record1[attributeName];
                    record1Value = record1[attributeName];
                }
                else if (record1[attributeName] is OptionSetValue)
                {
                    record1Value = record1.GetAttributeValue<OptionSetValue>(attributeName).Value;
                    record1Label = record1.FormattedValues[attributeName];
                }
                else if (record1[attributeName] is bool ||
                         record1[attributeName] is decimal ||
                         record1[attributeName] is double ||
                         record1[attributeName] is int ||
                         record1[attributeName] is DateTime)
                {
                    record1Value = record1[attributeName];
                    record1Label = record1.FormattedValues.Contains(attributeName)
                        ? record1.FormattedValues[attributeName]
                        : record1[attributeName].ToString();
                }
                else if (record1[attributeName] is EntityReference)
                {
                    record1Value = record1.GetAttributeValue<EntityReference>(attributeName).Id;
                    record1Label = record1.GetAttributeValue<EntityReference>(attributeName).Name;
                }
                else if (record1[attributeName] is Money)
                {
                    record1Value = record1.GetAttributeValue<Money>(attributeName).Value;
                    record1Label = record1.FormattedValues[attributeName];
                }
                else
                {
                    record1Label = $"{record1[attributeName].GetType().FullName} is not supported";
                }
            }

            object record2Value = null;
            string record2Label = null;

            if (record2.Contains(attributeName))
            {
                if (record2[attributeName] is string)
                {
                    record2Label = (string)record2[attributeName];
                    record2Value = record2[attributeName];
                }
                else if (record2[attributeName] is OptionSetValue)
                {
                    record2Value = record2.GetAttributeValue<OptionSetValue>(attributeName).Value;
                    record2Label = record2.FormattedValues[attributeName];
                }
                else if (record2[attributeName] is bool ||
                         record2[attributeName] is decimal ||
                         record2[attributeName] is double ||
                         record2[attributeName] is int ||
                         record2[attributeName] is DateTime)
                {
                    record2Value = record2[attributeName];
                    record2Label = record2.FormattedValues.Contains(attributeName)
                        ? record2.FormattedValues[attributeName]
                        : record2[attributeName].ToString();
                }
                else if (record2[attributeName] is EntityReference)
                {
                    record2Value = record2.GetAttributeValue<EntityReference>(attributeName).Id;
                    record2Label = record2.GetAttributeValue<EntityReference>(attributeName).Name;
                }
                else if (record2[attributeName] is Money)
                {
                    record2Value = record2.GetAttributeValue<Money>(attributeName).Value;
                    record2Label = record2.FormattedValues[attributeName];
                }
                else
                {
                    record2Label = $"{record2[attributeName].GetType().FullName} is not supported";
                }
            }

            outData.Record1FieldDisplayValue = record1Label;
            outData.Record2FieldDisplayValue = record2Label;

            outData.IsEqual = record1Value != null && record2Value != null && record1Value.Equals(record2Value);

            return outData;
        }

        [DataContract]
        public class InData
        {
            [DataMember] 
            public string entityType { get; set; }
            [DataMember] 
            public Guid Id1 { get; set; }
            [DataMember] 
            public Guid Id2 { get; set; }
        }

        [DataContract]
        public class OutData
        {
            [DataMember] 
            public string FieldLabel { get; set; }
            [DataMember]
            public string AttributeType { get; set; }
            [DataMember]
            public bool IsPrimary { get; set; }
            [DataMember]
            public bool IsCustom { get; set; }
            [DataMember] 
            public bool IsEqual { get; set; }
            [DataMember] 
            public string Record1FieldDisplayValue { get; set; }
            [DataMember] 
            public string Record2FieldDisplayValue { get; set; }
        }
    }
}
