import 'package:flutter/material.dart';
import '../../../core/theme/app_theme.dart';

class ParticipateTab extends StatelessWidget {
  const ParticipateTab({super.key});

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      backgroundColor: AppColors.background,
      body: Center(child: Text('Participar')),
    );
  }
}
