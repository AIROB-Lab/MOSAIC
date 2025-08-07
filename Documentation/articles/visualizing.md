# Visualizing

Although it may seem secondary, *visualization* of data is absolutely paramount in scientific software development. In MOSAIC , a significant effort has been dedicated to ensure that the data flow is visualized in an interpretable and accessible manner. Visualization in this context covers at least three main aspects:

1. **Post-Experiment Data Analysis:**  
   Use MATLAB or your preferred tools (such as R or Python) to analyze and visualize data dumped during experiments. This process—plotting, scattering, and box-plotting your data—is essential to truly understand system behavior and to confirm or reject your hypotheses. In many research groups, especially in scientific computing, the workflow starts with visualization: generate your plots first, and build your insights and publications around the data representations.
   
2. **Live Block Visualization via Control Panels:**  
   Each block's control panel (cp) enables live inspection of the signals and parameters flowing through the block. This is vital for verifying that each block is functioning as expected and at the correct rate. The project provides several built-in monitors, such as spider plots and standard time plots, located in the `Visualization` folder. However, you are encouraged to innovate and implement new forms of live visualization to suit your needs. Ideas include live 3D plots with pan/zoom/rotate, incremental PCA with 3D projections, or live boxplots. Even a simple text label can serve to verify numerical data flow.

3. **Live Block Table Visualization:**  
   Perhaps the most important form of visualization in MOSAIC is the live update of the block table, which graphically represents the system’s structure and current state. The block table is refreshed by a global timer (running lazily at 20Hz) that updates the display to reflect the real-time status of all blocks. The current implementation includes:
   
   - **Block Representation:** Each block is shown as a vertex in a graph, displaying the block’s name and desired rate.
   - **Color Coding:**  
     - **Grey**: Idle (no recent output).
     - **Green**: Active, running near the desired rate.
     - **Red**: Lagging, actual rate deviates more than 5% from the desired rate.
     - **Magenta**: Stumbling, computation within the block is slower than 1/(desired rate) seconds.
   - **Graph Edges:** Edges between blocks represent the input/output relationships. Each edge is labeled either with the actual firing rate and a status flag (Y/n for idle, lagging, stumbling), or with an `x` if the destination block is not currently *subscribed* to the source block.

The block table provides immediate feedback on system connectivity and block health. For example, the presence of an `x` on an edge means a block is not currently subscribed to another, which is critical for optimizing CPU usage (e.g., by unsubscribing from fast data providers when not needed).

#### System Design Notes

- **Many-to-Many I/O:**  
  The architecture supports many-to-many relationships: a block may have multiple inputs and multiple outputs, and loops are permitted. To retain flexibility and simplicity, each block is designed with a single input handler (`OnNewInput`) and a single output method (`SendOutput`).  
  - If block A receives inputs from N sources, it decides how to process data based on the sender or value.
  - If block A needs to send different types of outputs, this typically indicates a need to split its functionality into multiple blocks, each with a single, well-defined output type.

- **Block Rate Awareness:**  
  Each block maintains awareness of its actual firing rate versus the desired rate. This makes it easy to identify performance issues—idle blocks, lagging blocks, and blocks that are stumbling under computational load—directly from the color-coding in the block table.

#### Practical Recommendations

- **Idle Blocks:**  
  Shown in grey—indicates no recent output.
- **Active (Healthy) Blocks:**  
  Shown in green—running near or at desired rate.
- **Lagging Blocks:**  
  Shown in red—actual output rate is less than 95% of desired rate.
- **Stumbling Blocks:**  
  Shown in magenta—internal computations are too slow, risking data loss or instability.

Lagging and stumbling are independent conditions; a block can stumble without lagging, or lag without stumbling. Either scenario signals the need for investigation—colored feedback helps you immediately spot and address issues in the processing chain.

