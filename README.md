## Gunrig VR  Meta Quest 2 (Unity 2022.3.59f1, Meta XR Core SDK 74.0.0)

Скрипты VR-реализации проекта виртуального тура по мандале Гунриг для Meta Quest 2.
Контекст проекта: [Виртуальный тур по мандале Гунриг](https://arigus.tv/news/culture/160300-virtualnyy-tur-po-mandale-gunrig-prezentovali-v-ulan-ude/).

### Скрипты
- LODLightmapOptimizer.cs  массовая настройка Scale in Lightmap и LightmapParameters для LOD-групп под выбранным родителем; кнопка в инспекторе.
- LODOptimizer.cs  упрощение LOD до 2 уровней, пороги перехода, отключение вклада LOD1 в GI, применение ко всем найденным группам.
- MenuButtonController.cs  перезапуск сцены по A (правый контроллер) и авто-ресет при снятии HMD после задержки.
- MusicSwitcher.cs (AudioSwitcherOptimized)  плавное переключение от вступительного клипа к циклическому фону (fade-out/in).
- PlayerMovementSound.cs  звук шагов/движения: автозапуск по скорости, fade-in/out, луп.
- SculptureInfoPanel.cs  показ/скрытие инфопанелей и доп. объектов у скульптур по дистанции игрока с плавной анимацией и управлением светом.
- SimpleVRLocomotion.cs  передвижение через OVRInput + CharacterController: мульти-лучевой грaундчек, гравитация, следование рельефу, защита от сфер.
- XButtonToggleObjects.cs  однократное переключение наборов объектов по X (левый контроллер), кулдаун, логи.

### Ссылки
- Репозиторий: https://github.com/dbrgvx/VR-Gunrig
- Публикация: https://arigus.tv/news/culture/160300-virtualnyy-tur-po-mandale-gunrig-prezentovali-v-ulan-ude/

### Стек
- Unity 2022.3.59f1
- Meta XR Core SDK 74.0.0
- Платформа: Meta Quest 2
