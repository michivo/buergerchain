using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using FreieWahl.Voting.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Digests;

namespace FreieWahl.Application.VotingResults
{
    public class VotingChainBuilder : IVotingChainBuilder
    {
        private SHA512Managed _digest;

        public VotingChainBuilder()
        {
            _digest = new SHA512Managed();
        }

        public string GetGenesisValue(Question q)
        {
            var serializedQuestion = _GetSerializedQuestion(q);
            var hash = _digest.ComputeHash(serializedQuestion);
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(1)); // version
            result.AddRange(hash);
            return Convert.ToBase64String(result.ToArray());
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
                var jsonAnswer = new JObject();
                jsonAnswer["Text"] = answerOption.AnswerText;
                jsonAnswer["Id"] = answerOption.Id;
                var jsonAnswerDetails = new JArray();
                foreach (var answerDetail in answerOption.Details)
                {
                    var jsonAnswerDetail = new JObject();
                    jsonAnswerDetail["Type"] = (int) answerDetail.DetailType;
                    jsonAnswerDetail["Value"] = answerDetail.DetailValue;
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
            var hash = _digest.ComputeHash(serializedVote);
            var result = new List<byte>();
            result.AddRange(BitConverter.GetBytes(1)); // version
            result.AddRange(hash);
            return Convert.ToBase64String(result.ToArray());
        }

        private byte[] _GetSerializedVote(Vote vote)
        {
            var jsonVote = new JObject();
            jsonVote["QuestionIndex"] = vote.QuestionIndex;
            jsonVote["Token"] = vote.Token;
            jsonVote["SignedToken"] = vote.SignedToken;
            jsonVote["Timestamp"] = vote.TimestampData;
            jsonVote["PreviousBlockSignature"] = vote.PreviousBlockSignature;
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
