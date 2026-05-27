import os

def process_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        lines = f.readlines()
        
    new_lines = []
    changed = False
    
    for line in lines:
        stripped = line.strip()
        
        # Sadece // ile başlayan ama /// veya endpoint belirtenleri (GET, POST vb) koruyalım.
        # XAML'daki <!-- --> leri de temizleyebiliriz ama XAML içinde "<!-- Kiralama Butonu -->" gibi kısa başlıklar var, onlara dokunmuyoruz.
        
        if stripped.startswith("//") and not stripped.startswith("///"):
            lower = stripped.lower()
            if not any(x in lower for x in ["get ", "post ", "put ", "delete ", "patch "]):
                # Endpoint tanımı veya XML comment değilse sil (hocaya gidecek temiz kod)
                changed = True
                continue
                
        new_lines.append(line)
        
    if changed:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.writelines(new_lines)
        print(f"Cleaned: {filepath}")

for root, _, files in os.walk("."):
    for file in files:
        if file.endswith(".cs"):
            process_file(os.path.join(root, file))
