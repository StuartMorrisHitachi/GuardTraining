using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

public class Shared {
    private Dictionary<string, string> environment;
    public Shared() {
        var envVars = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(@"..\..\..\..\env.json"));
        environment = new Dictionary<string,string>();
        foreach (var pair in envVars) {
            environment.Add(pair.Key, pair.Value);
        }
    }

    public string GetEnvironment(string key) {
        return environment[key];
    }
}