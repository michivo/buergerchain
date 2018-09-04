using System;
using System.Collections.Generic;
using System.Text;
using FreieWahl.Voting.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;

namespace FreieWahl.Application.VotingResults
{
    public class VotingChainBuilder : IVotingChainBuilder
    {
        private const int FormatVersion = 1;
        
        private IDigest _CreateDigest()
        {
            return new KeccakDigest();
        }

        public string GetGenesisValue(Question q)
        {
            var serializedQuestion = _GetSerializedQuestion(q);
            var digest = _CreateDigest();
            var digestSize = digest.GetDigestSize();

            digest.Reset();
            digest.BlockUpdate(serializedQuestion, 0, serializedQuestion.Length);
            var version = BitConverter.GetBytes(FormatVersion);
            var result = new byte[digestSize + 4];
            digest.DoFinal(result, 4);
            Array.Copy(version, result, 4);
            return Convert.ToBase64String(result);
        }

        private byte[] _GetSerializedQuestion(Question question)
        {
            JObject o = new JObject();
            o["Text"] = question.QuestionText;
            o["Index"] = question.QuestionIndex;
            var details = _SerializeQuestionDetails(question);

            o["Details"] = details;
            var jsonAnswers = _SerializeAnswerOptions(question);

            o["AnswerOptions"] = jsonAnswers;
            var resultString = o.ToString(Formatting.None);

            return Encoding.UTF8.GetBytes(resultString);
        }

        private static JArray _SerializeAnswerOptions(Question question)
        {
            var jsonAnswers = new JArray();
            foreach (var answerOption in question.AnswerOptions)
            {
                var jsonAnswer = new JObject
                {
                    ["Text"] = answerOption.AnswerText,
                    ["Id"] = answerOption.Id
                };
                var jsonAnswerDetails = new JArray();
                foreach (var answerDetail in answerOption.Details)
                {
                    var jsonAnswerDetail = new JObject
                    {
                        ["Type"] = (int) answerDetail.DetailType,
                        ["Value"] = answerDetail.DetailValue
                    };
                    jsonAnswerDetails.Add(jsonAnswerDetail);
                }

                jsonAnswer["Details"] = jsonAnswerDetails;
                jsonAnswers.Add(jsonAnswer);
            }

            return jsonAnswers;
        }

        private static JArray _SerializeQuestionDetails(Question question)
        {
            var details = new JArray();
            foreach (var questionDetail in question.Details)
            {
                var jsonDetail = new JObject();
                jsonDetail["Type"] = (int) questionDetail.DetailType;
                jsonDetail["Value"] = questionDetail.DetailValue;
                details.Add(jsonDetail);
            }

            return details;
        }

        public string GetSignature(Vote v)
        {
            var serializedVote = _GetSerializedVote(v);

            var digest = _CreateDigest();
            var digestSize = digest.GetDigestSize();

            digest.Reset();
            digest.BlockUpdate(serializedVote, 0, serializedVote.Length);
            var version = BitConverter.GetBytes(FormatVersion);
            var result = new byte[digestSize + 4];
            digest.DoFinal(result, 4);
            Array.Copy(version, result, 4);
            return Convert.ToBase64String(result);
        }

        private byte[] _GetSerializedVote(Vote vote)
        {
            var jsonVote = new JObject
            {
                ["QuestionIndex"] = vote.QuestionIndex,
                ["Token"] = vote.Token,
                ["SignedToken"] = vote.SignedToken,
                ["Timestamp"] = vote.TimestampData,
                ["PreviousBlockSignature"] = vote.PreviousBlockSignature
            };
            var jsonAnswers = new JArray();
            foreach (var answer in vote.SelectedAnswerIds)
            {
                jsonAnswers.Add(answer);
            }

            jsonVote["Answers"] = jsonAnswers;

            return Encoding.UTF8.GetBytes(jsonVote.ToString(Formatting.None));
        }

        public void CheckChain(Question q, IReadOnlyList<Vote> votes)
        {
            if (votes == null || votes.Count == 0)
                return;

            var lastBlockSignature = GetGenesisValue(q);
            foreach (var vote in votes)
            { // ATM there is only one version, so verification is trivial
                if(vote.PreviousBlockSignature != lastBlockSignature)
                    throw new Exception("Invalid signature at vote with id " + vote.VotingId);

                lastBlockSignature = GetSignature(vote);
            }
        }
    }
}
