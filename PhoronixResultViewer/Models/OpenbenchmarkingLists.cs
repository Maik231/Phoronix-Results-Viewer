using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;

namespace PhoronixResultViewer.Models;

public static class OpenbenchmarkingLists
{
    private static List<TestSuite> _testSuites = [];
    
    public static async Task<List<TestSuite>> GetTestSuites()
    {
        if (_testSuites.Count > 0)
        {
            return _testSuites;
        }
        
        if (File.Exists("testSuites.json"))
        {
            var json = await File.ReadAllTextAsync("testSuites.json");

            _testSuites = JsonSerializer.Deserialize<List<TestSuite>>(json);
            
            return _testSuites;
        }
        
        var httpClient = new HttpClient();
        var parser = new HtmlParser();

        var response = await httpClient.GetAsync("https://openbenchmarking.org/suites");
    
        response.EnsureSuccessStatusCode();
    
        var html = await response.Content.ReadAsStringAsync();
    
        var document = parser.ParseDocument(html);
        var cells = document.QuerySelectorAll("div.row div.col-sm-6 h4 a");
        var suitePaths = cells.Select(m => m.GetAttribute("href")).ToList();
        
        suitePaths.Add("/suite/pts/single-threaded");
        
        foreach (var suitePath in suitePaths)
        {
            response = await httpClient.GetAsync("https://openbenchmarking.org" + suitePath);
        
            response.EnsureSuccessStatusCode();
    
            html = await response.Content.ReadAsStringAsync();
        
            document = parser.ParseDocument(html);
        
            var suiteName = document.QuerySelector("div.col-sm-9.col-md-10.main h1").TextContent;
        
            var suiteTests = document.QuerySelectorAll("ul li h3 a").Select(i => i.GetAttribute("href").Split('&')[0].Split('/')[3]);
        
            _testSuites.Add(new TestSuite(suiteName, suiteTests.ToList()));
        }
    
        await File.WriteAllTextAsync("testSuites.json", JsonSerializer.Serialize(_testSuites));
        
        return _testSuites;
    }
    
    public static List<string> AVX512benchmarkList =
    [
        "mt-dgemm",
        "aobench",
        "blender",
        "chia-vdf",
        "cp2k",
        "dav1d",
        "embree",
        "gcrypt",
        "gromacs",
        "intel-mlc",
        "oidn",
        "john-the-ripper",
        "jpegxl",
        "kripke",
        "lammps",
        "lczero",
        "litert",
        "llama-cpp",
        "localscore",
        "minibude",
        "mnn",
        "n-queens",
        "namd",
        "ncnn",
        "nginx",
        "ollama",
        "onednn",
        "onnx",
        "openapv",
        "openssl",
        "openvino",
        "openvino-genai",
        "openvkl",
        "ospray",
        "ospray-studio",
        "povray",
        "primesieve",
        "pytorch",
        "quadray",
        "rocksdb",
        "rust-mandel",
        "simdjson",
        "smhasher",
        "speedb",
        "stargate",
        "svt-av1",
        "svt-hevc",
        "svt-jpeg-xs",
        "svt-vp9",
        "tensorflow",
        "tensorflow-lite",
        "mrbayes",
        "tnn",
        "uvg266",
        "vvenc",
        "webp",
        "webp2",
        "whisper-cpp",
        "xmrig",
        "z3"
    ];
}