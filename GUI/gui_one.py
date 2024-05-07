import tkinter as tk
import subprocess

class Application(tk.Frame):
    def __init__(self, master=None):
        super().__init__(master)
        self.master = master
        self.process = None
        self.create_widgets()

    def create_widgets(self):
        self.start_button = tk.Button(self.master, text="Start", command=self.start_script, font=("Arial", 24), width=10, height=2)
        self.start_button.pack(side="left", padx=20, pady=20)

        self.stop_button = tk.Button(self.master, text="Stop", command=self.stop_script, font=("Arial", 24), width=10, height=2)
        self.stop_button.pack(side="right", padx=20, pady=20)

    def start_script(self):
        if self.process is None:
            self.process = subprocess.Popen(["../csharp_scripts/survive-websocketd"])

    def stop_script(self):
        if self.process is not None:
            self.process.terminate()
            # self.process.kill()
            self.process = None

root = tk.Tk()
root.title("Script Controller")
app = Application(master=root)
app.mainloop()