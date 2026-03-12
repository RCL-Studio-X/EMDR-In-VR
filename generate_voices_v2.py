import os
from openai import OpenAI
import base64

# Replace YOUR_OPENAI_API_KEY with your actual OpenAI API key.
# Alternatively, set it as an environment variable: export OPENAI_API_KEY="your-key"
client = OpenAI(api_key=os.environ.get("OPENAI_API_KEY", "YOUR_OPENAI_API_KEY"))

# We use the new Audio API (gpt-4o-audio-preview) which allows us to prompt it 
# for specific pacing, tone, and inflection, resulting in much more natural speech.

def generate_breathing_audio(prompt, text, output_filename):
    print(f"Generating audio for: '{text}'...")
    
    response = client.chat.completions.create(
        model="gpt-4o-audio-preview",
        modalities=["text", "audio"],
        audio={"voice": "shimmer", "format": "mp3"},
        messages=[
            {
                "role": "user",
                "content": f"{prompt} Now, please say: {text}"
            }
        ]
    )

    # Decode the base64 audio response and save it
    audio_base64 = response.choices[0].message.audio.data
    audio_bytes = base64.b64decode(audio_base64)
    
    with open(output_filename, "wb") as f:
        f.write(audio_bytes)
        
    print(f"Saved to {output_filename}")

if __name__ == "__main__":
    # The prompt explicitly guides the model's inflection, pacing, and tone.
    system_prompt = "You are a calming meditation guide. Speak softly, peacefully, and deliberately. Draw the words out just slightly longer than conversational speed—it should take about 3 seconds to say."
    
    os.makedirs("Assets/Sounds", exist_ok=True)
    
    # generate_breathing_audio(
    #     system_prompt, 
    #     "Breathe in.", 
    #     "Assets/Sounds/breathe_in.mp3"
    # )
    
    generate_breathing_audio(
        system_prompt, 
        "Breathe out.", 
        "Assets/Sounds/breathe_out.mp3"
    )
    
    print("\nDone! Now play the scene in Unity to hear the new audio.")
