# Deploy Pharos API to Google Cloud Run
Write-Host "Deploying Pharos API to Google Cloud Run..."
gcloud run deploy pharos-api `
  --source . `
  --region us-central1 `
  --allow-unauthenticated `
  --set-env-vars="FIREBASE_PROJECT_ID=pharos-ai-demo,GEMINI_API_KEY=YOUR_GEMINI_API_KEY,GOOGLE_CLOUD_PROJECT=pharos-ai-demo"
