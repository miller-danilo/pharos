# Pharos AI | Job Fraud Shield & Cover Letter Generator

Pharos is an AI-powered security assistant designed to protect job seekers from employment scams, phishing vectors, and fraudulent offers. If a scanned vacancy is safe, the application helps candidates align their profiles and craft highly personalized cover letters using elite AI models.

## Repository Structure
*   **`frontend/`**: The client web application built with **Astro**, **Vue 3 (Composition API)**, and **Vanilla CSS/Tailwind**.
*   **`backend/`**: The security analysis API built on **ASP.NET Core (.NET 8)** and **Google Cloud Firestore**.

---

## Getting Started Locally

### Prerequisites
*   [Node.js](https://nodejs.org/) (v22.12.0 or higher)
*   [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [pnpm](https://pnpm.io/) (`npm install -g pnpm`)

---

### 1. Running the Backend API
1. Navigate to the backend directory:
   ```bash
   cd backend
   ```
2. Build and run the ASP.NET Core application:
   ```bash
   dotnet run --project Pharos.Api
   ```
   *The API will start by default on `http://localhost:5295`.*

---

### 2. Running the Frontend Application
1. Navigate to the frontend directory:
   ```bash
   cd frontend
   ```
2. Install package dependencies:
   ```bash
   pnpm install
   ```
3. Launch the development server:
   ```bash
   pnpm run dev
   ```
   *The client dashboard will start by default on `http://localhost:4321`.*

---

## Features
*   **Anti-Fraud Shield:** Employs advanced LLM scanning to identify external redirects, financial traps, or suspicious platform migration requests.
*   **Resume Alignment:** Evaluates candidate profiles (CVs) against job details to highlight transferable achievements.
*   **Offline Mode Integration:** Supports toggling local Firebase Emulators (Authentication & Firestore) and mock AI engines for zero-cost, offline developer runs.
