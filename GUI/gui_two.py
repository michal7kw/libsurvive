import tkinter as tk
import subprocess

class Application(tk.Frame):
    def __init__(self, master=None):
        super().__init__(master)
        self.master = master
        self.process = None
        self.ip_address = tk.StringVar(value="127.0.0.1")
        self.create_widgets()

    def create_widgets(self):
        self.ip_label = tk.Label(self.master, text="IP Address:")
        self.ip_label.pack()

        self.ip_entry = tk.Entry(self.master, textvariable=self.ip_address)
        self.ip_entry.pack()

        self.start_button = tk.Button(self.master, text="Start", command=self.start_script, font=("Arial", 24), width=10, height=2)
        self.start_button.pack(side="left", padx=20, pady=20)

        self.stop_button = tk.Button(self.master, text="Stop", command=self.stop_script, font=("Arial", 24), width=10, height=2)
        self.stop_button.pack(side="right", padx=20, pady=20)

    def start_script(self):
        if self.process is None:
            ip_address = self.ip_address.get()
            command = f"websocketd --passenv OPENBLAS_NUM_THREADS --passenv HOME --port 8080 --address {ip_address} /home/michal/Documents/Githubs/libsurvive/bin/survive-cli --record-stdout --no-record-imu --report-covariance 30"
            self.process = subprocess.Popen(command, shell=True)

    def stop_script(self):
        if self.process is not None:
            self.process.kill()
            self.process = None

root = tk.Tk()
root.title("Script Controller")
app = Application(master=root)
app.mainloop()