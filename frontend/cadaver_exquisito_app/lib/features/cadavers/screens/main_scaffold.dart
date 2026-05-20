import 'package:flutter/material.dart';
import 'package:phosphor_flutter/phosphor_flutter.dart';
import '../../../core/theme/app_theme.dart';
import 'participate_tab.dart';
import 'archive_tab.dart';

class MainScaffold extends StatefulWidget {
  const MainScaffold({super.key});

  @override
  State<MainScaffold> createState() => _MainScaffoldState();
}

class _MainScaffoldState extends State<MainScaffold> {
  int _currentIndex = 0;

  static const _tabs = [ParticipateTab(), ArchiveTab()];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: _tabs[_currentIndex],
      bottomNavigationBar: Container(
        decoration: const BoxDecoration(
          color: AppColors.surface,
          boxShadow: [
            BoxShadow(
                color: AppColors.shadowDark,
                offset: Offset(0, -2),
                blurRadius: 6),
          ],
        ),
        child: BottomNavigationBar(
          currentIndex: _currentIndex,
          onTap: (i) => setState(() => _currentIndex = i),
          backgroundColor: AppColors.surface,
          selectedItemColor: AppColors.primary,
          unselectedItemColor: AppColors.textMuted,
          elevation: 0,
          items: const [
            BottomNavigationBarItem(
              icon: PhosphorIcon(PhosphorIconsRegular.pencilSimple),
              activeIcon: PhosphorIcon(PhosphorIconsFill.pencilSimple),
              label: 'Participar',
            ),
            BottomNavigationBarItem(
              icon: PhosphorIcon(PhosphorIconsRegular.bookOpen),
              activeIcon: PhosphorIcon(PhosphorIconsFill.bookOpen),
              label: 'Archivo',
            ),
          ],
        ),
      ),
    );
  }
}
