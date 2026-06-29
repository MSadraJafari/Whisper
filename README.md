# Whisper

<p align="center">
  <img src="https://img.shields.io/badge/.NET-WPF-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET WPF" />
  <img src="https://img.shields.io/badge/Language-C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white" alt="C#" />
  <img src="https://img.shields.io/badge/Protocol-TCP-0EA5E9?style=for-the-badge" alt="TCP" />
  <img src="https://img.shields.io/badge/Platform-Windows-1E3A8A?style=for-the-badge&logo=windows&logoColor=white" alt="Windows" />
</p>

<p align="center">
  <b>Whisper</b> is a modern desktop messenger built with WPF and TCP sockets.<br/>
  It is designed to feel fast, clean, calm, and personal.
</p>


---

## ✨ About

Whisper is a Windows chat application with a dark modern UI, animated screens, profile setup, file sending, and a chat layout inspired by modern messengers.

The goal of the project is simple: build a desktop messenger that looks good, feels smooth, and stays lightweight.

---

## 🌟 Features

- Real-time TCP messaging
- Modern dark UI
- Smooth page and window transitions
- Username, phone number, biography, birthday, and nickname setup
- Profile picture selection and cropping
- Saved messages section
- Chat list with custom styling
- File sending support
- Message history handling
- Clean profile panel
- Animated intro and setup flow

---

## 🛠 Built With
 - C#
 - WPF
 - .NET
 - TCP Sockets
 - JSON Serialization
 - Custom XAML Styles
 - Bitmap / Image processing


---
## 📁 Project Structure

```text
Whisper/
├── Devices/                 # WPF client application
│   ├── InfoPages/           # Setup wizard pages
│   ├── IntroPages/          # Intro screens
│   ├── Styles/              # Reusable XAML styles
│   ├── Required-Data/       # Assets and required local data
│   ├── MainWindow.xaml
│   ├── InfoWindow.xaml
│   ├── IntroWindow.xaml
│   └── CropImage.xaml
│
├── Model/                   # Shared data models
│
├── Server/                  # TCP chat server application
│
├── WpfChatApp.sln           # Solution file
└── README.md
```


---

## 🚀 Getting Started
Requirements:
 - Windows 10 / 11
 - Visual Studio 2022
 - .NET Desktop Development workload
 - Run the project
 - Clone the repository
 - Open the solution in Visual Studio
 - Restore NuGet packages if needed
 - Build and run the project

---

## 🔐 Notes
 - Whisper is still being developed and improved step by step.
 The focus is on:
   - clean and readable code
   - smooth UI
   - stable networking
   - better user experience
   - lightweight desktop performance

---

## 📌 Future Ideas
 - better message templates
 - message bubbles with attachments
 - online/offline status updates
 - typing indicator
 - better profile editing
 - theming improvements
 - local cache / history improvements



---

  ## 👤 Author


<p align="center">
  Made with ❤️, ☕ and countless late-night debugging sessions by <b>Sadra Jafari</b>
</p>

---



## 📄 License

This project is open for personal and educational use.
