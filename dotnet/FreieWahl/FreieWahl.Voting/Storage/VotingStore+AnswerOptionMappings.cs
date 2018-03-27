using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreieWahl.Voting.Models;
using Google.Cloud.Datastore.V1;

namespace FreieWahl.Voting.Storage
{
    partial class VotingStore
    {
        private static Value ToEntities(AnswerOption[] answerOptions)
        {
            var result = new ArrayValue();
            if (answerOptions != null)
            {
                result.Values.Add(answerOptions.Select(ToEntity));
            }

            return result;
        }

        private static Value ToEntity(AnswerOption answerOption)
        {
            return new Entity
            {
                ["AnswerId"] = answerOption.Id,
                ["AnswerText"] = answerOption.AnswerText,
                ["AnswerDetails"] = ToEntities(answerOption.Details)
            };
        }

        private static Value ToEntities(AnswerDetail[] answerDetails)
        {
            var result = new ArrayValue();
            if (answerDetails != null)
            {
                result.Values.Add(answerDetails.Select(ToEntity));
            }

            return result;
        }

        private static Value ToEntity(AnswerDetail answerDetail)
        {
            return new Entity
            {
                ["DetailValue"] = answerDetail.DetailValue,
                ["DetailType"] = (int)answerDetail.DetailType
            };
        }

        private static AnswerOption[] FromAnswerOptionEntity(Value value)
        {
            if (value?.ArrayValue?.Values == null || value.ArrayValue.Values.Count == 0)
                return new AnswerOption[0];

            var array = value.ArrayValue;

            return array.Values.Select(x => new AnswerOption
            {
                Id = x.EntityValue["AnswerId"].StringValue,
                AnswerText = x.EntityValue["AnswerText"].StringValue,
                Details = FromAnswerDetails(x.EntityValue["AnswerDetails"])
            }).ToArray();
        }

        private static AnswerDetail[] FromAnswerDetails(Value value)
        {
            if(value?.ArrayValue?.Values == null || value.ArrayValue.Values.Count == 0)
                return new AnswerDetail[0];

            var array = value.ArrayValue;

            return array.Values.Select(x => new AnswerDetail
            {
                DetailValue = x.EntityValue["DetailValue"].StringValue,
                DetailType = (AnswerDetailType)x.EntityValue["DetailType"].IntegerValue
            }).ToArray();
        }
    }
}
