import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/theme/app_theme.dart';
import '../providers/completed_cadavers_provider.dart';

class ArchiveTab extends ConsumerWidget {
  const ArchiveTab({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final completed = ref.watch(completedCadaversProvider);
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: AppColors.background,
        elevation: 0,
        title: Text(
          'Archivo',
          style: GoogleFonts.playfairDisplay(
              color: AppColors.textDark, fontWeight: FontWeight.bold),
        ),
      ),
      body: completed.when(
        data: (cadavers) => cadavers.isEmpty
            ? Center(
                child: Text(
                  'Aún no hay historias completas.',
                  style: GoogleFonts.dmSans(color: AppColors.textMuted),
                ),
              )
            : ListView.separated(
                padding: const EdgeInsets.all(16),
                itemCount: cadavers.length,
                separatorBuilder: (_, __) => const SizedBox(height: 12),
                itemBuilder: (context, i) {
                  final c = cadavers[i];
                  return GestureDetector(
                    onTap: () => Navigator.push(
                      context,
                      MaterialPageRoute(
                        builder: (_) => _StoryReaderScreen(
                            cadaverId: c.id, title: c.title),
                      ),
                    ),
                    child: SoftCard(
                      child: Row(
                        children: [
                          Expanded(
                            child: Text(
                              c.title,
                              style: GoogleFonts.playfairDisplay(
                                  fontSize: 16,
                                  color: AppColors.textDark),
                            ),
                          ),
                          const Icon(Icons.arrow_forward_ios_rounded,
                              size: 14, color: AppColors.primary),
                        ],
                      ),
                    ),
                  );
                },
              ),
        loading: () =>
            const Center(child: CircularProgressIndicator()),
        error: (e, _) => Center(child: Text('Error: $e')),
      ),
    );
  }
}

class _StoryReaderScreen extends ConsumerWidget {
  const _StoryReaderScreen(
      {required this.cadaverId, required this.title});
  final String cadaverId;
  final String title;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final fragments = ref.watch(fullCadaverProvider(cadaverId));
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: AppColors.background,
        elevation: 0,
        leading: const BackButton(color: AppColors.textDark),
        title: Text(
          title,
          style: GoogleFonts.playfairDisplay(
              color: AppColors.textDark, fontSize: 16),
        ),
      ),
      body: fragments.when(
        data: (frags) => ListView.builder(
          padding: const EdgeInsets.all(20),
          itemCount: frags.length,
          itemBuilder: (context, i) {
            final f = frags[i];
            return Padding(
              padding: const EdgeInsets.only(bottom: 24),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    f.content,
                    style: GoogleFonts.playfairDisplay(
                        fontSize: 16,
                        color: AppColors.textDark,
                        height: 1.8),
                  ),
                  const SizedBox(height: 6),
                  Text(
                    '— ${f.authorName}',
                    style: GoogleFonts.dmSans(
                        fontSize: 12,
                        color: AppColors.textMuted,
                        fontStyle: FontStyle.italic),
                  ),
                  if (i < frags.length - 1) ...[
                    const SizedBox(height: 16),
                    Divider(
                        color: AppColors.textMuted.withValues(alpha: 0.2)),
                  ],
                ],
              ),
            );
          },
        ),
        loading: () =>
            const Center(child: CircularProgressIndicator()),
        error: (e, _) => Center(child: Text('Error: $e')),
      ),
    );
  }
}
