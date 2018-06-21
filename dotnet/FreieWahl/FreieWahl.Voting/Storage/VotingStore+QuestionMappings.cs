using System.Collections.Generic;
using System.Linq;
using FreieWahl.Voting.Common;
using FreieWahl.Voting.Models;
using Google.Cloud.Datastore.V1;

namespace FreieWahl.Voting.Storage
{
    partial class VotingStore
    {
        private static ArrayValue ToEntities(IEnumerable<Question> questions)
        {
            var result = new ArrayValue();
            if (questions != null)
            {
                result.Values.Add(questions.Select(ToEntity));
            }

            return result;
        }

        private static Value ToEntity(Question question)
        {
            return new Entity
            {
                ["Id"] = question.Id,
                ["Status"] = (int)question.Status,
                ["QuestionText"] = question.QuestionText,
                ["Details"] = ToEntities(question.Details),
                ["AnswerOptions"] = ToEntities(question.AnswerOptions)
            };
        }

        private static ArrayValue ToEntities(IEnumerable<QuestionDetail> questionDetails)
        {
            var result = new ArrayValue();
            if (questionDetails != null)
            {
                result.Values.Add(questionDetails.Select(ToEntity));
            }

            return result;
        }

        private static Value ToEntity(QuestionDetail questionDetail)
        {
            return new Entity
            {
                ["DetailValue"] = questionDetail.DetailValue,
                ["DetailType"] = (int)questionDetail.DetailType
            };
        }

        private static List<Question> FromQuestionsEntity(Value entity)
        {
            if (entity?.ArrayValue?.Values == null || entity.ArrayValue.Values.Count == 0)
                return new List<Question>();

            var array = entity.ArrayValue;

            return array.Values.Select(value =>
            {
                Entity e = value.EntityValue;
                var result = new Question
                {
                    Id = e.Properties.ContainsKey("Id") ? e["Id"].IntegerValue : IdHelper.GetId(),
                    Status = e.Properties.ContainsKey("Status") ? (QuestionStatus)e["Status"].IntegerValue : QuestionStatus.InPreparation,
                    QuestionText = e["QuestionText"].StringValue,
                    AnswerOptions = FromAnswerOptionEntity(e["AnswerOptions"]),
                    Details = FromQuestionDetailEntity(e["Details"])
                };
                return result;
            }).ToList();
        }

        private static List<QuestionDetail> FromQuestionDetailEntity(Value value)
        {
            if (value?.ArrayValue?.Values == null || value.ArrayValue.Values.Count == 0)
                return new List<QuestionDetail>();

            var array = value.ArrayValue;

            return array.Values.Select(x =>
                new QuestionDetail
                {
                    DetailValue = x.EntityValue["DetailValue"].StringValue,
                    DetailType = (QuestionDetailType)x.EntityValue["DetailType"].IntegerValue
                }).ToList();
        }
    }
}
