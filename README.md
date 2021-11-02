# High Level Visual Scripting System
Offers game designers an intuitive way to express ideas and realize game prototypes without dependencing on programming. 

Based on Unity's GraphView technology
Simple and powerful C# node API to create new nodes and custom views.

```CSharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;

[System.Serializable, NodeMenuItem("Operations/Sub")] // Add the node in the node creation context menu
public class SubNode : BaseNode
{
    [Input(name = "A")]
    public float                inputA;
    [Input(name = "B")]
    public float                inputB;

    [Output(name = "Out")]
    public float				output;

    public override string		name => "Sub";

    // Called when the graph is process, process inputs and assign the result in output.
    protected override void Process()
    {
        output = inputA - inputB;
    }
}
```

## Gallery


### Node connection menu
![](https://user-images.githubusercontent.com/6877923/89330117-12f67d80-d690-11ea-9b62-f878b86b8342.gif)

### Node creation menu
![](https://user-images.githubusercontent.com/6877923/58935811-893adf80-876e-11e9-9f69-69ce51a432b8.png)

### Graph Parameters
![](https://user-images.githubusercontent.com/6877923/90035202-d6470980-dcc1-11ea-92e0-a754820bdc55.png)

### Node Inspector
![](https://user-images.githubusercontent.com/6877923/87306684-ac5ec380-c518-11ea-9346-1ed47e8cd016.gif)


### Drag And Drop Objects
![CreateNodeFromObject](https://user-images.githubusercontent.com/6877923/110240003-20d3f000-7f4a-11eb-8adc-e52340945b74.gif)
