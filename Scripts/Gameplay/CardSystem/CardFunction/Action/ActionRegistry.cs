using System.Collections.Generic;
using UnityEngine;

public class ActionRegistry
{
    private Dictionary<string, IActionExecutor> executors = new Dictionary<string, IActionExecutor>();

    public void Register(string key, IActionExecutor executor)
    {
        executors[key] = executor;
    }

    public void Execute(string key, CardActionDTO cardAction, ref int damage, CharacterBase enemy)
    {
        if (executors.TryGetValue(key, out var executor))
        {
            executor.Execute(cardAction, ref damage, enemy);
        }
    }
}
