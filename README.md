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

---

## 🚀 Deployment Guide (Production Setup)

This project is configured to run on **Render** (backend API) and **Firebase Hosting** (frontend Astro dashboard).

### 1. Deploy C# Backend to Render
1. Set up a new **Web Service** on [Render](https://render.com/).
2. Link your public GitHub repository (`pharos-public`).
3. Set the service parameters:
   * **Root Directory:** `backend`
   * **Runtime:** `Docker` (loads `backend/Dockerfile` and binds to `8080`)
4. Add the following **Environment Variables**:
   * `ASPNETCORE_ENVIRONMENT` = `Production`
   * `GEMINI_API_KEY` = *Your Google Gemini API Key*
   * `FIREBASE_PROJECT_ID` = *Your Firebase Project ID*
   * `GOOGLE_CLOUD_PROJECT` = *Your Google Cloud Project ID*
   * `GOOGLE_CREDENTIALS_JSON` = *The complete JSON service account key for your GCP project (having Cloud Datastore Owner role)*.

### 2. Deploy Frontend to Firebase Hosting
1. Inside `frontend/`, create a `.env.production` file:
   ```env
   PUBLIC_API_URL=https://your-backend-api-url.onrender.com
   PUBLIC_FIREBASE_API_KEY=YOUR_REAL_FIREBASE_API_KEY
   PUBLIC_FIREBASE_AUTH_DOMAIN=YOUR_PROJECT_ID.firebaseapp.com
   PUBLIC_FIREBASE_PROJECT_ID=YOUR_PROJECT_ID
   PUBLIC_FIREBASE_STORAGE_BUCKET=YOUR_PROJECT_ID.appspot.com
   PUBLIC_FIREBASE_MESSAGING_SENDER_ID=YOUR_SENDER_ID
   PUBLIC_FIREBASE_APP_ID=YOUR_APP_ID
   ```
2. Build and deploy using the Firebase CLI:
   ```bash
   cd frontend
   pnpm install
    # 2. Since .firebaserc is ignored to keep your production project IDs private, 
    # associate your local workspace with your Firebase project ID first:
    firebase use --add

    # 3. Build and deploy:
    pnpm run build
    firebase deploy --only hosting
   ```
