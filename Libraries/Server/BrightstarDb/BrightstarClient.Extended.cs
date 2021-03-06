﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Serilog.Log;

namespace Libraries.Server.BrightstarDb
{
    public partial class BrightstarClient : IBrightstarClient
    {
        public async Task<bool> RevertLastTransaction(string storename)
        {
            return await ClientCall(Task.Run(() =>
            {
                var lastCommitPoint = _brightstarClient.GetCommitPoints(storename, 0, 1).FirstOrDefault();
                if (lastCommitPoint == null)
                {
                    return false;
                }

                _brightstarClient.RevertToCommitPoint(storename, lastCommitPoint);
                return true;
            }, CancellationTokenSource.Token));
        }

        public async Task<IEnumerable<(ulong id, DateTime commitDate)>> ListCommitPoints(string storename, int limit = 100)
        {
            return await ClientCall(Task.Run(() =>
            {
                var commitPointInfoList = _brightstarClient.GetCommitPoints(storename, 0, limit);
                return commitPointInfoList?.Select(commit => (commit.Id, commit.CommitTime)).ToList();
            }, CancellationTokenSource.Token));
        }

        public async Task<bool> RevertToCommitPoint(string storename, ulong commitId)
        {
            return await ClientCall(Task.Run(() =>
            {
                var commitPointInfoList = _brightstarClient.GetCommitPoints(storename, 0, 100);
                if (commitPointInfoList == null)
                {
                    return false;
                }

                var commitPoint = commitPointInfoList.FirstOrDefault(c => c.Id == commitId);
                if (commitPoint == null)
                {
                    Warning($"Cannot find commit with id {commitId}");
                    return false;
                }

                _brightstarClient.RevertToCommitPoint(storename, commitPoint);
                return true;
            }, CancellationTokenSource.Token));
        }

        public async Task<string> GetStatistics(string storeName)
        {
            return await ClientCall(Task.Run(() =>
            {
                var statistics = _brightstarClient.GetStatistics(storeName);
                var mostUsedPredicates = statistics?.PredicateTripleCounts?.OrderByDescending(c => c.Value).Take(10);
                if (mostUsedPredicates == null)
                {
                    return "";
                }

                string predicatesWithCounter = null;
                foreach (var predicate in mostUsedPredicates)
                {
                    predicatesWithCounter += $"\n{predicate.Key}: {predicate.Value}";
                }

                return $"Last commit:\nId: {statistics.CommitId}, date: {statistics.CommitTimestamp}\n" +
                       $"TotalTripleCount: {statistics.TotalTripleCount}\n" +
                       $"10 most used predicates: {predicatesWithCounter}";
            }, CancellationTokenSource.Token));
        }

        public async Task<bool> ConsolidateDataset(string storeName)
        {
            return await ClientCall(Task.Run(() =>
            {
                _brightstarClient.ConsolidateStore(storeName);
                return true;
            }, CancellationTokenSource.Token));
        }
    }
}
