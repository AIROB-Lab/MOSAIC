# Examples

The MOSAIC suite comes with example YAML configuration files, each designed to introduce you to the system, one step at a time. These examples, found in the mosaic_program directory, range from basic to more advanced setups.

We encourage you to select and run each configuration file in turn. As you do so, observe how the block graph is constructed, how blocks interact, and how control panels (cps) and signals behave. Use the opportunity to explore and experiment: connect a Myo armband or try the Blender arm models, inspect the code for each block, and don’t hesitate to make changes to see their effects.

#### Example 1: A Minimal Chain (cfg1_simple_example.yaml)

This is a minimal **sanity check** configuration for MOSAIC (formerly *iM-blocks*), also discussed at the start of this guide. It builds a simple signal processing chain:

- A **fast timer** running at 2 kHz  
- A **decimator timer** that reduces the rate to 200 Hz  
- A **10-dimensional sinusoidal signal generator**  
- Two **function blocks**:  
  - One that **squares** the signal  
  - One that **doubles** it  

In the YAML file, `squaredSignal` subscribes directly to `sinusoidSignal`, while `doubledSignal` uses a `-` prefix in its `Inputs` field to indicate *non-subscription*. This is visualized in the block table as an `x` on the link from `sinusoidSignal` to `doubledSignal`.

> Subscriptions can only be changed by editing the configuration and restarting the system — runtime changes are not supported in this simple example.

#####  Running the Example

Start the **fast timer** via its control panel (left-click the block) or by right-clicking it. The active dataflow path turns **green**, indicating live signal propagation.

Use each block's control panel to observe how the signal changes — from its original sinusoidal form to squared and doubled outputs.

##### Tips

- **Path logging:** Uncomment the `Path` fields in the YAML file to enable logging. MOSAIC will save output data to `Path\blockName.txt`.  
  Try this with `sinusoidSignal` and `squaredSignal` to generate log files like `sinusoidSignal.txt`.

- **Restart behavior:** If you stop and restart the fast timer, the chain may briefly appear **red** (“lagging”). This is expected — rate estimation is influenced by idle time. The system stabilizes as buffers refill.

##### Timing Sanity Check – Debug vs. Release Configurations

A key step in verifying your setup is to examine the timing precision of your block outputs. This is easily done by exporting data dumps from MOSAIC (iM) and analyzing them in MATLAB.

##### How to Check Timing

1. **Load your dump file in MATLAB:**  
   Each dump file contains the timestamp as its first column, followed by the output data columns (e.g., for a 10D sinusoidal signal, you'll see 11 columns in total).

2. **Calculate the time difference between consecutive samples:**  
   This reveals the sampling interval, which should match the inverse of the `DesiredRate` in your config.

   ```matlab
   data = load('sinusoidSignal.txt');
   deltaT = diff(data(:,1));
   mean(deltaT)
   std(deltaT)
   plot(deltaT,'k.'); grid on;```


#### Example: EMG Signal Processing Pipeline (cfg2_EMG_raw2ARV.yaml)

This example showcases a typical surface electromyography (sEMG) processing workflow using the Myo bracelet (Thalmic Labs, Canada) and the MOSAIC software suite. It demonstrates how to transform raw EMG data into Averaged Rectified Values (ARVs), a widely used signal representation in myocontrol applications.

##### Block Graph Overview

![Block Graph: EMG Processing Pipeline](/images/3.png)

##### Signal Processing Flow

1. **Raw EMG Acquisition**  
   The Myo device is sampled at **200 Hz** using a `ScheduledTimer` block (`timer_myo`), generating the raw EMG signal (`raw_EMG`).

2. **Preprocessing Pipeline**  
   The signal undergoes the following operations:
   - **Amplification** by a factor of 5 (`amplified_raw_EMG`)
   - **Rectification** via absolute value (`rectified_raw_EMG`)
   - **Low-pass filtering** using a second-order Butterworth filter at 2 Hz (`ARV_LPF`)

   This produces the **Averaged Rectified Value (ARV)** — a smoothed envelope of EMG activity.

3. **Downsampling**  
   The filtered ARV is resampled at two different rates using `DecimatorTimer` blocks:
   - `ARV_fast`: 25 Hz (fast response)
   - `ARV_slow`: 5 Hz (slower, more stable)

> Instead of using three separate timers (200 Hz, 25 Hz, 5 Hz), we use one `ScheduledTimer` and two `DecimatorTimers` to save system resources and maintain synchronization.

##### Getting Started

1. **Start the system**:
   - Launch the 200 Hz main timer (`timer_myo`)
   - Then start the Myo device block (`raw_EMG`)
2. **Open each block’s Control Panel (CP)** to inspect and monitor signals in real-time at each stage of the pipeline.
3. **Experiment**:
   - Try modifying block parameters in the YAML config.
   - Add a `Path` field to any block to log outputs (e.g., `ARV_fast.txt`), which can be analyzed in MATLAB or Python.

##### Timing Sanity Check & Synchronization

When downsampling signals at different rates (e.g., 5 Hz vs 25 Hz), it’s important to ensure they remain synchronized.

Each dump file includes timestamps. You can use these to interpolate lower-frequency signals onto the higher-rate timeline, aligning both streams for comparison.


#### Example: Storing Labeled EMG Clusters (`cfg3_store_clusters.yaml`)

In this example, we extend the sEMG processing pipeline to include **label-controlled data collection**. The signal chain remains similar to the previous example, computing Averaged Rectified Values (ARV) from Myo armband input. This time, however, the processed signal is sent to a `Buffer` block, which captures segments of data based on **user-triggered labels**.

---

#####  Workflow Overview

![BlockTable: Cluster Storage](/images/9.png)

The left branch of the block graph (from `timer_myo` down to `ARV`) replicates the EMG processing steps from previous examples — leveraging modularity in MOSAIC’s design.

To begin:

1. **Start** the main timer and the Myo block.
2. Open the control panels (CPs) of the `Trigger` (`capture_data`) and `Buffer` (`cluster_storage`) blocks.
3. Focus the `capture_data` panel and use the arrow keys:
   - **Left/Right arrows** to select a label: `rest`, `flex_wrist`, or `extend_wrist`
   - **Down arrow** to start/stop data capture


##### Capturing Clusters with Labels

When you press and hold the **down arrow**, the `Buffer` begins collecting ARV data. You’ll see the link between `ARV` and `cluster_storage` turn from an x to a live update rate (e.g., `25 Hz`) — indicating active data flow.

Once you release the **down arrow**, collection stops and the edge returns to x (no subscription). Each captured segment is labeled according to the current selection in the `Trigger` block.

For example:
- Select `flex_wrist` using the arrow keys
- Flex your wrist and hold it
- Press the down arrow for ~2 seconds (≈ 50 samples at 25 Hz)
- Release the key, then relax

This will store a labeled EMG cluster for the “flex_wrist” action.

##### Buffer Management

The `cluster_storage` block maintains a growing internal list `dB`, containing pairs of:

- A **label vector** (e.g., `[1]` for `flex_wrist`)
- A **data matrix** with the corresponding ARV samples

From the Buffer CP, you can:

- **Save** the collected dataset to file
- **Clear** the buffer to restart data collection

This enables iterative, structured data gathering — ideal for building labeled datasets for machine learning, classification, or analysis in MATLAB/Python.

##### Pro Tips

- The Buffer is **non-subscribed** to `ARV` by default. It only activates during labeled capture events — efficient and clean.
- You can increase the capture duration or modify labels directly in the YAML configuration.
- Use `Path` fields if you’d like to save signal snapshots during collection for offline analysis.


#### Example: Batch Myocontrol System (cfg4_batch_myocontrol.yaml)

This example demonstrates a batch machine learning-based myocontrol pipeline using MOSAIC. It maps 8 sEMG channels to 12 activation values and controls a Blender arm model through a trained predictor.

##### Setup

1. Launch the Blender arm model (`3DHands_right.exe` or `3DHands_left.exe`).
2. Start this configuration in your MOSAIC project.
3. Activate the main timer and the Myo armband.

##### System Overview

- EMG → ARV preprocessing (as in previous examples)
- ARV is sent to both:
  - A `Buffer` for labeled data collection
  - A `BatchPredictor` for real-time prediction
- A `Switch` toggles between:
  - Sinusoidal test input
  - Learned predictions
- Final output is sent to the `BlenderArm` via a `ControlAlgorithm`.

##### Data Collection

1. Open `capture_data` (Trigger) and `cluster_storage` (Buffer).
2. Use left/right arrows to select action (e.g., `rest`, `flex`, `extend`).
3. Press and hold the down arrow to record labeled ARV data.
4. Release to stop recording. Repeat for multiple actions.

Each label corresponds to a 12D activation vector (defined in the Trigger block).

##### Model Training

1. Open the `batch_learn` control panel.
2. Click "Build model" to train on collected data.
3. Use the `Switch` to select the predictor output as control input.

You should now see the Blender arm respond to your EMG activity.

##### Notes

- If predictions are unstable, collect more data or adjust filter/model settings.
- Use the control panels to clear the Buffer or reset the model.
- You can pause control by stopping the timer or switching back to `sinGen`.

##### Summary

This example demonstrates:

- ARV-based sEMG processing
- Structured data labeling and storage
- One-shot model training via `BatchPredictor`
- Real-time control of a virtual arm model


#### Example: Incremental Myocontrol (`cfg5_incremental_myocontrol.yaml`)

This example demonstrates real-time **incremental learning** for myocontrol. Unlike batch learning, no buffer is needed—new labeled data is used immediately to update the model.

##### Overview

This setup is simpler than Example 4:
- No `Buffer`
- A `Trigger` (`capture_data`) sends ground truth directly to an `IncrementalPredictor` (`incr_learn`)
- The system continuously learns and adapts without needing to restart or retrain from scratch

##### Instructions

1. Start the system and the Myo armband.
2. Open the control panels for `capture_data` and `incr_learn`.
3. Use the left/right arrow keys to select an action (e.g., `rest`, `flex`, `extend`, etc.).
4. Press and hold the **down arrow** to start learning. The prediction will update immediately in the predictor CP and on the Blender arm.

##### Adjusting the Model

If predictions are poor:
- Gather more data by repeating the corresponding action.
- New samples will be used instantly to refine the model.
- No need to reset or build a model manually.

![Incremental Learning Setup](/images/12.png)

#### Example: Batch Myocontrol with Stimulus-Controlled Data Collection (`cfg6_batch_with_stimulus.yaml`)

This configuration builds upon Example 4 (batch myocontrol), but replaces manual, trigger-based data collection with a **stimulus-driven** approach. A `Stimulus` block presents a timed sequence of target actions to the user, ensuring consistent and structured data acquisition across participants.

##### System Overview

![Stimulus-Controlled Setup](/images/13.png)

The system works as follows:

- A `Stimulus` block generates a predefined sequence of activation targets.
- These targets are sent to:
  - A **white Blender arm** (for user feedback)
  - A `StimTrigger`, which controls when labeled data is captured
- A `Buffer` (`cluster_storage`) stores ARV data only during the stimulus “capture” phases.
- After the stimulus sequence ends, the stored data can be used to train a `BatchPredictor`.

This ensures each participant performs the same sequence of movements for the same duration, improving experimental control and data consistency.

##### Running the System

1. Start the **timer** and **Myo** block as usual.
2. Open the control panels (CPs) for:
   - The `Stimulus` block (`visual_stim`)
   - The `Buffer` (`cluster_storage`)
3. Keep the **white Blender arm** in view to follow the visual cues.
4. Click **Start** in the stimulus CP.
5. The Blender arm will move through a sequence of actions (e.g., `rest`, `power`, `flex`).
6. Mimic the movements shown by the arm. The system captures 3 seconds of ARV data for each action.

Once the sequence completes, six labeled clusters will be stored in the `Buffer`.

##### Training and Prediction

1. Open the `BatchPredictor` CP (`batch_learn`).
2. Click **Build model** to train on the collected clusters.
3. The predicted outputs will now control the **skin-textured Blender arm** in real time.

This process is identical to Example 4, but now benefits from a fully automated and repeatable data collection setup.

##### Summary

- This setup replaces manual data labeling with stimulus-guided acquisition.
- Participants follow a visual cue (Blender arm) while ARV data is collected automatically.
- Enables consistent training sessions, ideal for user studies or demonstrations.

Use this approach when you want structured, repeatable control over training data — and less reliance on manual input during acquisition.


#### Example: Incremental Myocontrol with TAC Evaluation (`cfg7_incremental_with_TAC.yaml`)

This final example combines **incremental myocontrol** (as introduced in Example 5) with a formal **Target Achievement Control (TAC)** test. It evaluates how well the user can follow a predefined target trajectory using live myoelectric control.

##### Overview

The system consists of two parallel control pipelines:

- The **skin-textured Blender arm** is controlled by the user via an `IncrementalPredictor`.
- The **white Blender arm** plays out a predefined sequence of movements via a `Stimulus`.

The goal: match the white arm’s pose using the skin-textured arm. If the user holds the pose accurately for a short duration (e.g. 2 seconds), the task is considered successfully completed.

![TAC System Overview](/images/14.png)

##### Workflow

1. **Start the timer and Myo armband.**
2. Open the `ground_truth` CP and begin capturing labeled examples (e.g., rest, power, flex, extend, pronate).
   - These are used to train the `IncrementalPredictor`.
   - You’ll see the skin-textured arm respond immediately as you build or refine the model.
3. Observe that:
   - A **stronger low-pass filter** (cutoff = 0.3 Hz) is used for prediction stability.
   - **Ridge Regression** is selected to allow smoother control and **action blending**.
     - For example, training only on basic actions still enables combined gestures like *power grasp + wrist extension*.

##### Performing the TAC Test

1. Open the control panels for:
   - `TAC_stim` (stimulus generator)
   - `increm_learning_TAC` (TAC comparator)
2. Click **Start** in the stimulus CP.
   - The **white arm** will begin performing target movements.
3. Your task: replicate each pose using the **skin-textured arm**.
4. If your prediction stays within the target tolerance (defined in the config) for ~2 seconds:
   - The system recognizes task completion and plays a confirmation beep.
   - The stimulus moves to the next task automatically.

#### Additional Examples
These are supplemental configurations with only minor modifications to the core examples presented above.

- Example 8 (cfg8_direct_control.yaml): Uses Incremental Ridge Regression with EMG input for real-time direct control of the Blender skin-textured arm via labeled wrist/finger actions.
- Example 9 (cfg9_stepwise_control.yaml): Same setup as Example 8 but uses StepwiseControl instead of DirectControl for discrete class-based actuation (e.g. open/close hand).
- Example 10 (cfg10_dual_device.yaml): Expands on Example 8 by using two Myo armbands for 16-channel EMG input, allowing more expressive and accurate direct control via Incremental Ridge Regression.
- Example 11 (cfg11_dual_stepwise.yaml): Builds on Example 10 by switching to StepwiseControl with the 16-channel EMG setup, enabling discrete multi-gesture control across both hands and wrist DOFs.
- Example 12 (cfg12_hannes_hand.yaml): Demonstrates real-time incremental myocontrol of the IIT Hannes Hand, using either Ridge Regression or Random Fourier Features (RFF) predictors with Myo-based EMG input.
- Example 13 (cfg13_udp_streaming.yaml): Extends Example 10 by streaming the control outputs via UDP, enabling integration with external systems like Unity or custom interfaces for prosthesis control and visualization.
