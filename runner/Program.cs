using runner.src.models;
using runner.src.services;


string path = Path.Combine(AppContext.BaseDirectory, "definitions", "def1.yml");
Job j1 = new(path);
JobService srv = new(j1);
srv.Run();
