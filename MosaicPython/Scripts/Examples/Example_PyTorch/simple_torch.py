import torch
import torch.nn as nn

class SimpleModel(nn.Module):
    def __init__(self, input_size):
        super().__init__()
        self.fc = nn.Linear(input_size, 1)

    def forward(self, x):
        return self.fc(x)

model = SimpleModel(input_size=5)
# ... train the model ...
torch.jit.save(torch.jit.script(model), "model.pt")