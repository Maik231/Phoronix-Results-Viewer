using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ReactiveUI;

namespace PhoronixResultViewer.Models;

public class TestSystem(string name, bool isBase, bool include) : ReactiveObject
{
    public bool Include
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = include;
    
    public bool IsBase
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = isBase;

    public string Name
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = name;
}