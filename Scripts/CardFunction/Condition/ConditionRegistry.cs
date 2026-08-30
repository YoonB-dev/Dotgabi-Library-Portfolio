using System.Collections.Generic;
using UnityEngine;

public class ConditionRegistry
{
    private Dictionary<string, IConditionEvaluator> evaluators = new ();

    public void Register(string key, IConditionEvaluator evaluator)
    {
        evaluators[key] = evaluator;
    }

    public bool Check(string key, CardActionDTO cardAction, Player player , GameObject enemy)
    {
        if (evaluators.TryGetValue(key, out var evaluator))
        {
            return evaluator.Evaluate(cardAction, player, enemy);
        }
        return false;
    }
}
