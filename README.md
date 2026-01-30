# Web Scraping CRM for Steam Community market

**An Angular 21 client integrated with a .NET 9 Web API backend**. This application helps you find items listed on the Steam Community Market (specifically for **Team Fortress 2**) at favorable price ranges—whether you’re investing or simply adding to your personal collection.

> **Disclaimer**: This project is **not** affiliated with, endorsed, or sponsored by Valve Software. It simply consumes publicly available endpoints provided by Valve.

---

## 🚀 Features
- 💰 **Price Scanning**: Quickly search for TF2 listings on the Steam Community Market within your desired price range.
- 🏷️ **Minimalistic UI**: A sleek interface built with Angular, Tailwind, and SASS for a focused user experience.
- ⏱️ **Real-Time Updates**: Fetch live listing and pricing data directly from the Steam Community Market.
- 📈 **Investment Insights**: Identify undervalued items that might be profitable for collectors or traders.

---

## 📥 Installation

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/ValsCodes/Steam-App-Angular-Server
   ```
2. **Install Angular Client Dependencies**:
    ```bash
    npm install
    ```
3. **Restore & Build the .NET 9 Web API**:
    ```bash
    dotnet restore
    dotnet build
    ```
4. **Run the Application**:
Terminal 1 (Web API):
   ```bash
   dotnet run
   ```
5. **Terminal 2 (Angular client)**:
   ```bash
    npm start
   ```
Open your browser at http://localhost:4200.

## 📌 Requirements
- 🏷️ Node.js (latest LTS recommended)
- 🎯 .NET 9 SDK
